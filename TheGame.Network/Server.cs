using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;

using Microsoft.Extensions.Logging;

namespace TheGame.Network;

public class Server
{
    private readonly ConcurrentBag<Connection> _newConnections = new();
    private readonly ConcurrentDictionary<Guid, Connection> _connections = new();
    private readonly IConnectionCallbacks _connectionCallbacks;
    private readonly IPAddress _ip;
    private readonly int _port;
    private readonly IServerCallbacks _serverCallbacks;
    private readonly ILogger<Server> _logger;

    public Server(IPAddress ip, int port, IServerCallbacks serverCallbacks, ILogger<Server> logger)
    {
        _connectionCallbacks = new ServerConnectionCallbacks(this);
        _ip = ip;
        _port = port;
        _serverCallbacks = serverCallbacks;
        _logger = logger;
    }

    public void StartTcpListeningThread()
    {
        var acceptTcpConnectionsThread = new Thread(AcceptTcpConnections);
        acceptTcpConnectionsThread.Start();
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

                var connection = new Connection(client, _connectionCallbacks, _logger);

                // TODO: Add to _newConnections
                var _ = Task.Run(() => HandleNewConnection(connection));

                _logger.LogInformation("New connection {0}", connection.Id);
            }
        }
        catch (Exception e)
        {
            _logger.LogInformation(e, "Stopping listener");
            listener.Stop();
        }
    }

    public async Task WriteAll(byte[] data)
    {
        var tasks = _connections.Select(connection => connection.Value.Write(data)).ToArray();

        _logger.LogInformation("Wrote to {0} connections", tasks.Length);

        await Task.WhenAll(tasks);
    }

    public void Disconnect(Connection connection)
    {
        if (!_connections.Remove(connection.Id, out _))
        {
            _logger.LogError("Could not remove connection {0} from dictionary", connection.Id);
        }

        _serverCallbacks.OnDisconnect(connection.Id);
    }

    private async Task HandleNewConnection(Connection connection)
    {
        if (_connections.TryAdd(connection.Id, connection))
        {
            await Task.WhenAll(connection.Start(), _serverCallbacks.OnConnection(connection));
        }
        else
        {
            connection.Stop();
        }
    }
}
