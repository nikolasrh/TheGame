using Microsoft.Extensions.Logging;

using TheGame.Network;
using TheGame.Protobuf;

namespace TheGame.Server;

public class ServerCallbacks : IServerCallbacks
{
    private readonly ILogger<ServerCallbacks> _logger;

    public ServerCallbacks(ILogger<ServerCallbacks> logger)
    {
        _logger = logger;
    }

    public async Task OnConnection(Connection newConnection, Network.Server server)
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

        var tasks = server.WriteAll(serverMessage);
        await Task.WhenAll(tasks);
    }

    public Task OnDisconnect(Network.Server server)
    {
        throw new NotImplementedException();
    }

    public async Task OnRead(byte[] data, Connection connection, Network.Server server)
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

                var tasks = server.WriteAll(serverMessage);
                await Task.WhenAll(tasks);
                break;
        }
    }
}
