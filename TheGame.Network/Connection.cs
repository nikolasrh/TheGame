using System.Buffers;
using System.Net.Sockets;
using System.Threading.Channels;

using Microsoft.Extensions.Logging;

namespace TheGame.Network;

public class Connection
{
    private readonly Channel<byte[]> _channel;
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly CancellationToken _cancellationToken;
    private readonly TcpClient _tcpClient;
    private readonly IConnectionCallbacks _connectionCallbacks;
    private readonly ILogger _logger;
    private readonly NetworkStream _networkStream;

    public Connection(TcpClient tcpClient, IConnectionCallbacks connectionCallbacks, ILogger logger)
    {
        _channel = Channel.CreateUnbounded<byte[]>(new UnboundedChannelOptions
        {
            AllowSynchronousContinuations = false,
            SingleReader = true,
            SingleWriter = true
        });
        tcpClient.SendTimeout = 5000;
        tcpClient.NoDelay = true;
        _cancellationToken = _cancellationTokenSource.Token;
        _tcpClient = tcpClient;
        _connectionCallbacks = connectionCallbacks;
        _logger = logger;
        _networkStream = tcpClient.GetStream();
    }

    public Guid Id { get; } = Guid.NewGuid();
    public bool Disconnected { get; private set; } = false;

    public void Write(byte[] bytes)
    {
        if (Disconnected) return;

        try
        {
            var prefix = BitConverter.GetBytes(bytes.Length);
            var bytesWithPrefix = prefix.Concat(bytes).ToArray();

            if (!_channel.Writer.TryWrite(bytesWithPrefix))
            {
                _logger.LogError("Error when writing to channel");
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error when writing to channel");
            Stop();
        }
    }

    public void Flush()
    {
        try
        {
            while (!Disconnected && _channel.Reader.TryRead(out var item))
            {
                _networkStream.Write(item);
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error when writing to network stream");
            Stop();
        }
    }

    public byte[]? Read()
    {
        try
        {
            if (Disconnected || !_networkStream.DataAvailable) return null;

            var prefixBuffer = new byte[4];
            var totalBytesReadToPrefixBuffer = _networkStream.Read(prefixBuffer, 0, prefixBuffer.Length);

            if (totalBytesReadToPrefixBuffer == 0) return null;

            var length = BitConverter.ToInt32(prefixBuffer);
            var buffer = new byte[length];

            var totalBytesReadToBuffer = _networkStream.Read(buffer, 0, length);

            if (totalBytesReadToBuffer == 0) return null;

            return buffer;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error when reading from network stream");
            Stop();
        }

        return null;
    }

    public void Stop()
    {
        if (Disconnected) return;

        Disconnected = true;

        _cancellationTokenSource.Cancel();

        try
        {
            _networkStream.Close();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error when closing network stream");
        }

        try
        {
            _tcpClient.Close();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error when closing tcp client");
        }

        _connectionCallbacks.OnDisconnect(this);
    }
}
