using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;

using Microsoft.Extensions.Logging;

namespace TheGame.Network;

public class Server
{
    private readonly TcpListener _listener;
    private readonly ConcurrentDictionary<Guid, Connection> _connections = new();
    private readonly IConnectionCallbacks _connectionCallbacks;
    private readonly IServerCallbacks _serverCallbacks;
    private readonly ILogger<Server> _logger;

    public Server(IPAddress ip, int port, IServerCallbacks serverCallbacks, ILogger<Server> logger)
    {
        _listener = new TcpListener(ip, port);
        _connectionCallbacks = new ServerConnectionCallbacks(this, serverCallbacks);
        _serverCallbacks = serverCallbacks;
        _logger = logger;
    }

    public async Task Start(CancellationToken cancellationToken)
    {
        _listener.Start();

        try
        {
            while (true)
            {
                _logger.LogInformation("Waiting for connection...");
                var client = await _listener.AcceptTcpClientAsync(cancellationToken);

                var connection = new Connection(client, _connectionCallbacks, _logger);

                // TODO: Decide if Task.Run is a good solution
                var _ = Task.Run(() => HandleNewConnection(connection));

                _logger.LogInformation("New connection {0}", connection.Id);
            }
        }
        catch
        {
            _listener.Stop();
        }
    }

    public Task WriteToAllConnections(byte[] data)
    {
        return WriteToConnections(data, _connection => true);
    }

    public Task WriteToConnections(byte[] data, Guid sender)
    {
        return WriteToConnections(data, connection => connection.Id != sender);
    }

    public async Task WriteToConnections(byte[] data, Func<Connection, bool> predicate)
    {
        var tasks = _connections
            .Select(connection => connection.Value)
            .Where(predicate)
            .Select(connection => connection.Write(data))
            .ToArray();

        _logger.LogInformation("Wrote to {0} connections", tasks.Length);

        await Task.WhenAll(tasks);
    }

    public void Disconnect(Connection connection)
    {
        if (!_connections.Remove(connection.Id, out _))
        {
            _logger.LogError("Could not remove connection {0} from dictionary", connection.Id);
        }
    }

    private async Task HandleNewConnection(Connection connection)
    {
        await _serverCallbacks.OnConnection(connection, this);

        if (_connections.TryAdd(connection.Id, connection))
        {
            await connection.Start();
        }
        else
        {
            await connection.Stop();
        }
    }
}
