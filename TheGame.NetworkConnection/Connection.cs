using System.Net.Sockets;
using System.Runtime.InteropServices;

using Microsoft.Extensions.Logging;

namespace TheGame.NetworkConnection;

public class Connection<TIncomingMessage, TOutgoingMessage>
{
    private static int PrefixBufferLength = 4;
    private static int BufferSize = 8192;
    private readonly TcpClient _tcpClient;
    private readonly IConnectionCallbacks<TIncomingMessage> _connectionCallbacks;
    private readonly IMessageSerializer<TIncomingMessage, TOutgoingMessage> _messageSerializer;
    private readonly ILogger _logger;
    private readonly byte[] _messageBuffer = new byte[8192];
    private bool Disconnected { get; set; } = false;
    private int NextIncomingMessageLength { get; set; } = 0;
    private int BufferPosition { get; set; } = 0;

    public Connection(
        IConnectionCallbacks<TIncomingMessage> connectionCallbacks,
        IMessageSerializer<TIncomingMessage, TOutgoingMessage> messageSerializer,
        TcpClient tcpClient,
        ILogger logger)
    {
        tcpClient.ReceiveBufferSize = BufferSize;
        tcpClient.SendBufferSize = BufferSize;

        tcpClient.SendTimeout = 5000;
        tcpClient.NoDelay = true;

        tcpClient.Client.NoDelay = true;
        tcpClient.Client.Blocking = false;

        _tcpClient = tcpClient;
        _connectionCallbacks = connectionCallbacks;
        _messageSerializer = messageSerializer;
        _logger = logger;
    }

    public void SendQueuedMessages()
    {
        if (BufferPosition == 0) return;

        Span<byte> buffer = _messageBuffer;
        var bufferWithMessage = buffer.Slice(0, BufferPosition);

        var bytesSent = _tcpClient.Client.Send(bufferWithMessage);

        if (bytesSent < BufferPosition)
        {
            ReadOnlySpan<byte> remainingBuffer = bufferWithMessage.Slice(bytesSent);
            remainingBuffer.CopyTo(buffer);
            BufferPosition = BufferPosition - bytesSent;
        }
        BufferPosition = 0;
    }

    public void QueueMessage(TOutgoingMessage message)
    {
        if (Disconnected) return;

        try
        {
            Span<byte> buffer = _messageBuffer;
            var messageSize = _messageSerializer.CalculateMessageSize(message);
            var totalSize = PrefixBufferLength + messageSize;

            if (BufferPosition + totalSize > BufferSize)
            {
                _logger.LogWarning("Message buffer is full, closing connection");
                Disconnect();
                return;
            }

            var prefixBuffer = buffer.Slice(BufferPosition, PrefixBufferLength);
            MemoryMarshal.Write(prefixBuffer, ref messageSize);
            BufferPosition += PrefixBufferLength;

            var messageBuffer = buffer.Slice(BufferPosition, messageSize);
            _messageSerializer.SerializeOutgoingMessage(message, messageBuffer);
            BufferPosition += messageSize;
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

                NextIncomingMessageLength = MemoryMarshal.AsRef<int>(prefixBuffer);
            }

            // It could happen that we can only read the length prefix, and that the message is not available yet.
            // The next attempt will not read out a length prefix, but instead use the existing one.
            if (_tcpClient.Client.Available < NextIncomingMessageLength) return false;

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
