using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;

using Microsoft.Extensions.Logging;

using TheGame.Core;
using TheGame.Core.Protobuf;
using TheGame.Server.Serialization;

namespace TheGame.Server;

public class ConnectionManager
{
    private readonly TcpListener _listener;
    private readonly ConcurrentDictionary<Guid, Connection> _connections = new();
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

    // TODO: Change to EventHandler
    private static async Task OnNewConnection(Connection newConnection, ConcurrentDictionary<Guid, Connection> existingConnections)
    {
        var playerId = newConnection.Id.ToString();
        var serverMessage = Serializer.Serialize(new ServerMessage
        {
            PlayerJoined = new PlayerJoined
            {
                Player = new Player
                {
                    Id = playerId
                }
            }
        });

        var tasks = existingConnections.Select(connection => connection.Value.Write(serverMessage));
        await Task.WhenAll(tasks);
    }

    // TODO: Change to EventHandler
    private async Task OnRead(Connection connection, byte[] data)
    {
        var clientMessage = Serializer.Deserialize(data);

        switch (clientMessage.MessageCase)
        {
            case ClientMessage.MessageOneofCase.SendChat:
                _logger.LogInformation("Received chat message: {0}", clientMessage.SendChat.Text);
                var playerId = connection.Id.ToString();

                var serverMessage = Serializer.Serialize(new ServerMessage
                {
                    Chat = new Chat
                    {
                        Player = new Player
                        {
                            Id = playerId
                        },
                        Text = clientMessage.SendChat.Text
                    }
                });

                var tasks = _connections.Select(connection => connection.Value.Write(serverMessage));
                await Task.WhenAll(tasks);
                break;
        }
    }

    private async Task HandleNewConnection(Connection connection)
    {
        await OnNewConnection(connection, _connections);

        if (!_connections.TryAdd(connection.Id, connection))
        {
            // TODO: End connection
        }

        await Task.WhenAll(connection.StartWriting(), connection.StartReading(OnRead));
    }
}
