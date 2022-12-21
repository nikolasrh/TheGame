using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;

using Microsoft.Extensions.Logging;

using TheGame.Common;
using TheGame.NetworkConnection;

namespace TheGame.NetworkServer;

public class Server<TIncomingMessage, TOutgoingMessage>
{
    private readonly ConcurrentDictionary<Guid, Connection<TIncomingMessage, TOutgoingMessage>> _connections = new();
    private readonly IPAddress _ip;
    private readonly int _port;
    private readonly IMessageSerializer<TIncomingMessage, TOutgoingMessage> _messageSerializer;
    private readonly ILogger _logger;

    public Server(
        IPAddress ip,
        int port,
        IMessageSerializer<TIncomingMessage, TOutgoingMessage> messageSerializer,
        ILogger<Server<TIncomingMessage, TOutgoingMessage>> logger)
    {
        _ip = ip;
        _port = port;
        _messageSerializer = messageSerializer;
        _logger = logger;
    }

    public delegate void ConnectionOpenedHandler(Guid connectionId);
    public delegate void ConnectionClosedHandler(Guid connectionId);
    public delegate void MessageReceivedHandler(Guid connectionId, TIncomingMessage message);
    public event ConnectionOpenedHandler? ConnectionOpened;
    public event ConnectionClosedHandler? ConnectionClosed;
    public event MessageReceivedHandler? MessageReceived;

    public void Start(Loop loop)
    {
        var acceptTcpConnectionsThread = new Thread(AcceptTcpConnections);
        acceptTcpConnectionsThread.Start();

        var checkForNewMessagesThread = new Thread(() => HandleIncomingMessages(loop));
        checkForNewMessagesThread.Start();
    }

    private void AcceptTcpConnections()
    {
        var listener = new TcpListener(_ip, _port);

        try
        {
            listener.Start();

            while (true)
            {
                _logger.LogInformation("Waiting for connection...");
                var client = listener.AcceptTcpClient();

                var connectionId = Guid.NewGuid();

                var connection = new Connection<TIncomingMessage, TOutgoingMessage>(_messageSerializer, client, _logger);

                connection.Disconnected += () => ConnectionClosed?.Invoke(connectionId);
                connection.MessageReceived += message => MessageReceived?.Invoke(connectionId, message);

                if (_connections.TryAdd(connectionId, connection))
                {
                    ConnectionOpened?.Invoke(connectionId);
                    _logger.LogInformation("Started connection {0}", connectionId);
                }
                else
                {
                    connection.Disconnect();
                }
            }
        }
        catch (Exception e)
        {
            _logger.LogInformation(e, $"Stopping {nameof(AcceptTcpConnections)}");
            listener.Stop();
        }
    }

    private void HandleIncomingMessages(Loop loop)
    {
        try
        {
            loop.Run(_ =>
            {
                foreach (var (_, connection) in _connections)
                {
                    connection.HandleIncomingMessages();
                }
            });
        }
        catch (Exception e)
        {
            _logger.LogInformation(e, $"Exception in {nameof(HandleIncomingMessages)}");
        }
    }

    public void QueueMessage(Guid connectionId, TOutgoingMessage message)
    {
        _connections[connectionId].QueueMessage(message);
    }

    public void QueueMessage(TOutgoingMessage message)
    {
        foreach (var (_, connection) in _connections)
        {
            connection.QueueMessage(message);
        }
    }

    public void SendMessages()
    {
        foreach (var (_, connection) in _connections)
        {
            connection.SendQueuedMessages();
        }
    }

    public void Disconnect(Guid connectionId)
    {
        if (_connections.Remove(connectionId, out var connection))
        {
            connection.Disconnect();
        }
        else
        {
            _logger.LogError("Could not remove connection {0} from dictionary", connectionId);
        }
    }
}
