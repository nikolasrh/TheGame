using System.Buffers;
using System.Net.Sockets;

using Microsoft.Extensions.Logging;

namespace TheGame.NetworkConnection;

public class Connection<TIncomingMessage, TOutgoingMessage>
{
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly CancellationToken _cancellationToken;
    private readonly TcpClient _tcpClient;
    private readonly IConnectionCallbacks<TIncomingMessage> _connectionCallbacks;
    private readonly IMessageSerializer<TIncomingMessage, TOutgoingMessage> _messageSerializer;
    private readonly ILogger _logger;

    public Connection(
        IConnectionCallbacks<TIncomingMessage> connectionCallbacks,
        IMessageSerializer<TIncomingMessage, TOutgoingMessage> messageSerializer,
        TcpClient tcpClient,
        ILogger logger)
    {
        tcpClient.ReceiveBufferSize = 8192;
        tcpClient.SendBufferSize = 8192;

        tcpClient.SendTimeout = 5000;
        tcpClient.NoDelay = true;

        tcpClient.Client.NoDelay = true;
        tcpClient.Client.Blocking = false;

        _cancellationToken = _cancellationTokenSource.Token;
        _tcpClient = tcpClient;
        _connectionCallbacks = connectionCallbacks;
        _messageSerializer = messageSerializer;
        _logger = logger;
    }

    public bool Disconnected { get; private set; } = false;
    private int NextIncomingMessageLength { get; set; } = 0;

    public void SendMessage(TOutgoingMessage message)
    {
        if (Disconnected) return;

        try
        {
            var serializedMessage = _messageSerializer.SerializeOutgoingMessage(message);
            var prefix = BitConverter.GetBytes(serializedMessage.Length);
            var bytesWithPrefix = prefix.Concat(serializedMessage).ToArray();
            _tcpClient.Client.Send(bytesWithPrefix); // TODO: Handle not all bytes being sent?
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error when writing to network stream");
            Disconnect();
        }
    }

    public void HandleIncomingMessages()
    {
        while (HandleIncomingMessage()) ;
    }

    private bool HandleIncomingMessage()
    {
        try
        {
            if (Disconnected || _tcpClient.Client.Available == 0) return false;

            if (NextIncomingMessageLength == 0)
            {
                const int PrefixBufferLength = 4;
                var prefixBuffer = new byte[PrefixBufferLength];

                var totalBytesReadToPrefixBuffer = _tcpClient.Client.Receive(prefixBuffer, 0, PrefixBufferLength, SocketFlags.None);

                if (totalBytesReadToPrefixBuffer == 0) return false;

                NextIncomingMessageLength = BitConverter.ToInt32(prefixBuffer);
            }

            var buffer = new byte[NextIncomingMessageLength];
            var totalBytesReadToBuffer = _tcpClient.Client.Receive(buffer, 0, NextIncomingMessageLength, SocketFlags.None);

            // It could happen that we can only read the length prefix, and that the message is not available yet.
            // The next attempt will not read out a length prefix, but instead use the existing one.
            if (totalBytesReadToBuffer == 0) return false;

            // Reset the length so that next attempt will read the length prefix.
            NextIncomingMessageLength = 0;
            var message = _messageSerializer.DeserializeIncomingMessage(buffer);
            _connectionCallbacks.OnMessage(message);
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error when reading from network stream");
            Disconnect();
            return false;
        }
    }

    public void Disconnect()
    {
        if (Disconnected) return;

        Disconnected = true;

        _cancellationTokenSource.Cancel();

        try
        {
            _tcpClient.Close();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error when closing tcp client");
        }

        _connectionCallbacks.OnDisconnect();
    }
}
