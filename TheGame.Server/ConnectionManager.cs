using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;

using Microsoft.Extensions.Logging;

using TheGame.Core;

namespace TheGame.Server;

public class ConnectionManager
{
    private readonly TcpListener _listener;
    private readonly ConcurrentDictionary<Guid, Connection> _connections = new();
    private readonly ConnectionCallbacks _connectionCallbacks;
    private readonly ConnectionManagerCallbacks _connectionManagerCallbacks;
    private readonly ILogger<ConnectionManager> _logger;

    public ConnectionManager(IPAddress ip, int port, ConnectionManagerCallbacks connectionManagerCallbacks, ILogger<ConnectionManager> logger)
    {
        _listener = new TcpListener(ip, port);
        _connectionCallbacks = new ConnectionCallbacks(this, connectionManagerCallbacks);
        _connectionManagerCallbacks = connectionManagerCallbacks;
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

    public async Task WriteAll(byte[] data)
    {
        var tasks = _connections.Select(connection => connection.Value.Write(data));
        await Task.WhenAll(tasks);
    }

    private async Task HandleNewConnection(Connection connection)
    {
        await _connectionManagerCallbacks.OnConnection(connection, this);

        if (!_connections.TryAdd(connection.Id, connection))
        {
            // TODO: End connection
        }

        await connection.Start();
    }
}
