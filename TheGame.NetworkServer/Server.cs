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
    private readonly IServerCallbacks<TIncomingMessage> _serverCallbacks;
    private readonly ILogger _logger;

    public Server(
        IPAddress ip,
        int port,
        IMessageSerializer<TIncomingMessage, TOutgoingMessage> messageSerializer,
        IServerCallbacks<TIncomingMessage> serverCallbacks,
        ILogger<Server<TIncomingMessage, TOutgoingMessage>> logger)
    {
        _ip = ip;
        _port = port;
        _messageSerializer = messageSerializer;
        _serverCallbacks = serverCallbacks;
        _logger = logger;
    }

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
                var connectionCallbacks = new ServerConnectionCallbacks<TIncomingMessage>(connectionId, _serverCallbacks);
                var connection = new Connection<TIncomingMessage, TOutgoingMessage>(connectionCallbacks, _messageSerializer, client, _logger);

                if (_connections.TryAdd(connectionId, connection))
                {
                    _serverCallbacks.OnConnection(connectionId);
                    _logger.LogInformation("New connection {0}", connectionId);
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

    public void SendMessage(Guid connectionId, TOutgoingMessage message)
    {
        _connections[connectionId].SendMessage(message);
    }

    public void SendMessage(TOutgoingMessage message)
    {
        foreach (var (_, connection) in _connections)
        {
            connection.SendMessage(message);
        }
    }

    public void Disconnect(Guid connectionId)
    {
        if (!_connections.Remove(connectionId, out _))
        {
            _logger.LogError("Could not remove connection {0} from dictionary", connectionId);
        }
        _serverCallbacks.OnDisconnect(connectionId);
    }
}
