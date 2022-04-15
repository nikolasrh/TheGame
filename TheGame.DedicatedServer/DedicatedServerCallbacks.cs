using Microsoft.Extensions.Logging;

using TheGame.Network;
using TheGame.Protobuf;

namespace TheGame.DedicatedServer;

public class DedicatedServerCallbacks : IServerCallbacks
{
    private readonly ILogger<DedicatedServerCallbacks> _logger;

    public DedicatedServerCallbacks(ILogger<DedicatedServerCallbacks> logger)
    {
        _logger = logger;
    }

    public async Task OnBeforeConnection(Connection connection, Server server)
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

        await server.WriteAll(serverMessage);
    }

    public async Task OnAfterConnection(Connection connection, Server server)
    {
        byte[]? data;
        while ((data = await connection.Read()) != null)
        {
            await OnRead(data, connection, server);
        }
    }

    public async Task OnDisconnect(Connection connection, Server server)
    {
        var playerId = connection.Id.ToString();
        var serverMessage = Serializer.Serialize(new ServerMessage
        {
            PlayerLeft = new PlayerLeft
            {
                Player = new Player
                {
                    Id = playerId
                }
            }
        });

        await server.WriteAll(serverMessage);
    }

    public async Task OnRead(byte[] data, Connection connection, Server server)
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

                await server.WriteAll(serverMessage);
                break;
        }
    }
}
