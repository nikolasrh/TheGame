using Microsoft.Extensions.Logging;

using TheGame.Network;
using TheGame.Protobuf;

namespace TheGame.Server;

public class ConnectionManagerCallbacks : IConnectionManagerCallbacks
{
    private readonly ILogger<ConnectionManagerCallbacks> _logger;

    public ConnectionManagerCallbacks(ILogger<ConnectionManagerCallbacks> logger)
    {
        _logger = logger;
    }

    public async Task OnConnection(Connection newConnection, ConnectionManager connectionManager)
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

        var tasks = connectionManager.WriteAll(serverMessage);
        await Task.WhenAll(tasks);
    }

    public Task OnDisconnect(ConnectionManager connectionManager)
    {
        throw new NotImplementedException();
    }

    public async Task OnRead(byte[] data, Connection connection, ConnectionManager connectionManager)
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

                var tasks = connectionManager.WriteAll(serverMessage);
                await Task.WhenAll(tasks);
                break;
        }
    }
}
