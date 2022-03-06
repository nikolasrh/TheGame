using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;

using Microsoft.Extensions.Logging;

using TheGame.Core;
using TheGame.Core.Serialization;

namespace TheGame.Server;

public class ConnectionManager
{
    private readonly TcpListener _listener;
    private readonly ConcurrentBag<(Connection, Task)> _connections = new();
    private readonly ILogger<ConnectionManager> _logger;

    public ConnectionManager(IPAddress ip, int port, ILogger<ConnectionManager> logger)
    {
        _listener = new TcpListener(ip, port);
        _logger = logger;
    }

    public async Task Run(CancellationToken cancellationToken)
    {
        _listener.Start();

        try
        {
            while (true)
            {
                _logger.LogInformation("Waiting for connection...");
                var client = await _listener.AcceptTcpClientAsync(cancellationToken);
                var connection = new Connection(client, _logger);
                var readingTask = ReadContinuously(connection, _logger);
                _connections.Add((connection, readingTask));
                _logger.LogInformation("New connection with guid {0}", connection.Id);
            }
        }
        catch
        {
            _listener.Stop();
        }
    }

    private static async Task ReadContinuously(Connection connection, ILogger logger)
    {
        while (true)
        {
            var data = await connection.Read();
            var gameEvent = Serializer.Deserialize(data);

            if (gameEvent.EventCase == GameEvent.EventOneofCase.ChatMessage)
            {
                logger.LogInformation("Received chat message: {0}", gameEvent.ChatMessage.Text);
            }
        }
    }
}
