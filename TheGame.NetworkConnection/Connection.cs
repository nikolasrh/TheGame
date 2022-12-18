using System.Net.Sockets;

using Microsoft.Extensions.Logging;

namespace TheGame.NetworkConnection;

public class Connection<TIncomingMessage, TOutgoingMessage>
{
    private const int PrefixBufferLength = 4;
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

            Span<byte> buffer = stackalloc byte[PrefixBufferLength + serializedMessage.Length];
            var prefixBuffer = buffer.Slice(0, PrefixBufferLength);
            var messageBuffer = buffer.Slice(PrefixBufferLength);

            if (BitConverter.TryWriteBytes(prefixBuffer, serializedMessage.Length) && serializedMessage.TryCopyTo(messageBuffer))
            {
                var bytesSent = _tcpClient.Client.Send(buffer);

                if (buffer.Length != bytesSent)
                {
                    _logger.LogError("Expected to send {0} bytes, but sent {1}", buffer.Length, bytesSent);
                    Disconnect();
                }
            }
            else
            {
                _logger.LogError("Error writing bytes to span");
                Disconnect();
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error sending message");
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
                Span<byte> prefixBuffer = stackalloc byte[PrefixBufferLength];

                var totalBytesReadToPrefixBuffer = _tcpClient.Client.Receive(prefixBuffer);

                if (totalBytesReadToPrefixBuffer == 0) return false;

                NextIncomingMessageLength = BitConverter.ToInt32(prefixBuffer);
            }

            // It could happen that we can only read the length prefix, and that the message is not available yet.
            // The next attempt will not read out a length prefix, but instead use the existing one.
            if (_tcpClient.Client.Available == 0) return false;

            Span<byte> buffer = stackalloc byte[NextIncomingMessageLength];
            var totalBytesReadToBuffer = _tcpClient.Client.Receive(buffer);

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
