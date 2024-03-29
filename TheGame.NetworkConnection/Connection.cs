using System.Net.Sockets;
using System.Runtime.InteropServices;

using Microsoft.Extensions.Logging;

namespace TheGame.NetworkConnection;

public class Connection<TIncomingMessage, TOutgoingMessage>
{
    private static int PrefixBufferLength = 4;
    private static int BufferSize = 8192;
    private readonly TcpClient _tcpClient;
    private readonly IMessageSerializer<TIncomingMessage, TOutgoingMessage> _messageSerializer;
    private readonly ILogger _logger;
    private readonly byte[] _messageBuffer = new byte[8192];
    private int _nextIncomingMessageLength = 0;
    private int _bufferPosition = 0;

    public Connection(
        IMessageSerializer<TIncomingMessage, TOutgoingMessage> messageSerializer,
        TcpClient tcpClient,
        ILogger logger)
    {
        tcpClient.ReceiveBufferSize = BufferSize;
        tcpClient.SendBufferSize = BufferSize;

        tcpClient.SendTimeout = 5000;
        tcpClient.NoDelay = true;

        tcpClient.Client.NoDelay = true;

        _tcpClient = tcpClient;
        _messageSerializer = messageSerializer;
        _logger = logger;
    }

    public delegate void DisconnectedEventHandler();
    public delegate void MessageReceivedEventHandler(TIncomingMessage message);
    public event DisconnectedEventHandler? Disconnected;
    public event MessageReceivedEventHandler? MessageReceived;

    public bool Connected { get => _tcpClient.Connected; }

    public bool Connect(string hostname, int port)
    {
        if (Connected)
        {
            _logger.LogInformation("Already connected.");
            return false;
        }

        try
        {
            _tcpClient.Client.Blocking = true;
            _tcpClient.Connect(hostname, port);
            _tcpClient.Client.Blocking = false;
            return true;
        }
        catch (Exception e)
        {
            _logger.LogInformation(e, "Could not connect to host.");
            return false;
        }
    }

    public void SendQueuedMessages()
    {
        if (!Connected || _bufferPosition == 0) return;

        Span<byte> buffer = _messageBuffer;
        var bufferWithMessage = buffer.Slice(0, _bufferPosition);

        try
        {
            var bytesSent = _tcpClient.Client.Send(bufferWithMessage);
            if (bytesSent < _bufferPosition)
            {
                ReadOnlySpan<byte> remainingBuffer = bufferWithMessage.Slice(bytesSent);
                remainingBuffer.CopyTo(buffer);
                _bufferPosition = _bufferPosition - bytesSent;
            }
            _bufferPosition = 0;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error sending message");
            Disconnect();
        }
    }

    public void QueueMessage(TOutgoingMessage message)
    {
        if (!Connected) return;

        Span<byte> buffer = _messageBuffer;
        var messageSize = _messageSerializer.CalculateMessageSize(message);
        var totalSize = PrefixBufferLength + messageSize;

        if (_bufferPosition + totalSize > BufferSize)
        {
            _logger.LogWarning("Message buffer is full, closing connection");
            Disconnect();
            return;
        }

        var prefixBuffer = buffer.Slice(_bufferPosition, PrefixBufferLength);
        MemoryMarshal.Write(prefixBuffer, ref messageSize);
        _bufferPosition += PrefixBufferLength;

        var messageBuffer = buffer.Slice(_bufferPosition, messageSize);
        _messageSerializer.SerializeOutgoingMessage(message, messageBuffer);
        _bufferPosition += messageSize;
    }


    public void HandleIncomingMessages()
    {
        while (HandleIncomingMessage()) ;
    }

    private bool HandleIncomingMessage()
    {
        try
        {
            if (!Connected || _tcpClient.Client.Available == 0) return false;

            if (_nextIncomingMessageLength == 0)
            {
                Span<byte> prefixBuffer = stackalloc byte[PrefixBufferLength];

                var totalBytesReadToPrefixBuffer = _tcpClient.Client.Receive(prefixBuffer);

                if (totalBytesReadToPrefixBuffer == 0) return false;

                _nextIncomingMessageLength = MemoryMarshal.AsRef<int>(prefixBuffer);
            }

            // It could happen that we can only read the length prefix, and that the message is not available yet.
            // The next attempt will not read out a length prefix, but instead use the existing one.
            if (_tcpClient.Client.Available < _nextIncomingMessageLength) return false;

            Span<byte> buffer = stackalloc byte[_nextIncomingMessageLength];
            var totalBytesReadToBuffer = _tcpClient.Client.Receive(buffer);

            // Reset the length so that next attempt will read the length prefix.
            _nextIncomingMessageLength = 0;
            var message = _messageSerializer.DeserializeIncomingMessage(buffer);

            try
            {
                MessageReceived?.Invoke(message);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error when invoking {0} event handler", nameof(MessageReceived));
            }
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
        if (Connected)
        {
            try
            {
                _tcpClient.Close();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error when closing tcp client");
            }
        }

        try
        {
            Disconnected?.Invoke();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error when invoking {0} event handler", nameof(Disconnected));
        }
    }
}
