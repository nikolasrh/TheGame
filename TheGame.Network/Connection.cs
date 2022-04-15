using System.IO.Pipelines;
using System.Net.Sockets;

using Microsoft.Extensions.Logging;

namespace TheGame.Network;

public class Connection
{
    private readonly Pipe _outgoing = new();
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly CancellationToken _cancellationToken;
    private readonly TcpClient _tcpClient;
    private readonly IConnectionCallbacks _connectionCallbacks;
    private readonly ILogger _logger;
    private readonly NetworkStream _networkStream;

    public Connection(TcpClient tcpClient, IConnectionCallbacks connectionCallbacks, ILogger logger)
    {
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

    public async Task Start()
    {
        await StartWriting();
    }

    public async Task Write(byte[] bytes)
    {
        if (Disconnected) return;

        try
        {
            var prefix = BitConverter.GetBytes(bytes.Length);
            var bytesWithPrefix = prefix.Concat(bytes).ToArray();

            await _outgoing.Writer.WriteAsync(bytesWithPrefix, _cancellationToken);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error when writing to pipe");
            Stop();
        }
    }

    private async Task StartWriting()
    {
        try
        {
            var reader = _outgoing.Reader;

            while (!Disconnected)
            {
                var result = await reader.ReadAsync(_cancellationToken);

                foreach (var memory in result.Buffer)
                {
                    await _networkStream.WriteAsync(memory, _cancellationToken);
                }

                reader.AdvanceTo(result.Buffer.End);
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error when writing to network stream");
            Stop();
        }
    }

    public async Task<byte[]?> Read()
    {
        try
        {
            while (!Disconnected)
            {
                var prefixBuffer = new byte[4];
                var totalBytesReadToPrefixBuffer = await _networkStream.ReadAsync(prefixBuffer, 0, prefixBuffer.Length, _cancellationToken);

                if (totalBytesReadToPrefixBuffer == 0) continue;

                var length = BitConverter.ToInt32(prefixBuffer);
                var buffer = new byte[length];

                var totalBytesReadToBuffer = await _networkStream.ReadAsync(buffer, 0, length, _cancellationToken);

                if (totalBytesReadToBuffer == 0) continue;

                return buffer;
            }
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
