using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;

using Microsoft.Extensions.Logging;

namespace TheGame.Network;

public class Server
{
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

    public void Start()
    {
        var acceptTcpConnectionsThread = new Thread(AcceptTcpConnections);
        acceptTcpConnectionsThread.Start();

        var readConnectionsThread = new Thread(ReadConnections);
        readConnectionsThread.Start();

        var flushConnectionsThread = new Thread(FlushConnections);
        flushConnectionsThread.Start();
    }

    private void FlushConnections()
    {
        try
        {
            while (true)
            {
                foreach (var (_, connection) in _connections)
                {
                    connection.Flush();
                }
            }
        }
        catch (Exception e)
        {
            _logger.LogInformation(e, $"Stopping {nameof(FlushConnections)}");
        }
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

                if (_connections.TryAdd(connection.Id, connection))
                {
                    _serverCallbacks.OnConnection(connection);
                    _logger.LogInformation("New connection {0}", connection.Id);
                }
                else
                {
                    connection.Stop();
                }
            }
        }
        catch (Exception e)
        {
            _logger.LogInformation(e, $"Stopping {nameof(AcceptTcpConnections)}");
            listener.Stop();
        }
    }

    private void ReadConnections()
    {
        try
        {
            while (true)
            {
                foreach (var (_, connection) in _connections)
                {
                    byte[]? data;
                    while ((data = connection.Read()) != null)
                    {
                        _serverCallbacks.OnConnectionRead(connection.Id, data);
                    }
                }
            }
        }
        catch (Exception e)
        {
            _logger.LogInformation(e, $"Stopping {nameof(ReadConnections)}");
        }
    }

    public async Task WriteAllAsync(byte[] data)
    {
        var tasks = _connections.Select(connection => connection.Value.WriteAsync(data)).ToArray();

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
}
