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

                var _ = Task.Run(() => HandleConnection(connection));

                _logger.LogInformation("New connection {0}", connection.Id);
            }
        }
        catch
        {
            _listener.Stop();
        }
    }

    private async Task HandleConnection(Connection connection)
    {
        var playerId = connection.Id.ToString();
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

        var _ = Task.Run(async () =>
        {
            var tasks = _connections.Select(connection => connection.Value.Write(serverMessage)).ToArray();
            await Task.WhenAll(tasks);
        });

        if (_connections.TryAdd(connection.Id, connection))
        {
            while (true)
            {
                var data = await connection.Read();
                var clientMessage = Serializer.Deserialize(data);
                HandleClientMessage(connection.Id, clientMessage);
            }
        }
    }

    private void HandleClientMessage(Guid connectionId, ClientMessage clientMessage)
    {
        switch (clientMessage.MessageCase)
        {
            case ClientMessage.MessageOneofCase.SendChat:
                _logger.LogInformation("Received chat message: {0}", clientMessage.SendChat.Text);
                var playerId = connectionId.ToString();

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

                var _ = Task.Run(async () =>
                {
                    var tasks = _connections.Select(connection => connection.Value.Write(serverMessage)).ToArray();
                    await Task.WhenAll(tasks);
                });
                break;
        }
    }
}
