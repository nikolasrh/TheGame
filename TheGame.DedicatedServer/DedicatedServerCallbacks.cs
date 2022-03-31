using Microsoft.Extensions.Logging;

using TheGame.Core;
using TheGame.Network;
using TheGame.Protobuf;

namespace TheGame.DedicatedServer;

public class DedicatedServerCallbacks : IServerCallbacks
{
    private readonly PlayerGameState _playerGameState;
    private readonly ILogger<DedicatedServerCallbacks> _logger;

    public DedicatedServerCallbacks(PlayerGameState playerGameState, ILogger<DedicatedServerCallbacks> logger)
    {
        _playerGameState = playerGameState;
        _logger = logger;
    }

    public Task OnConnection(Connection newConnection, Server server)
    {
        return Task.CompletedTask;
    }

    public async Task OnDisconnect(Connection connection, Server server)
    {
        var playerId = connection.Id.ToString();
        var serverMessage = Serializer.Serialize(new ServerMessage
        {
            PlayerLeft = new()
            {
                Player = new()
                {
                    Id = playerId
                }
            }
        });

        await server.WriteToAllConnections(serverMessage);
    }

    public async Task OnRead(byte[] data, Connection connection, Server server)
    {
        var clientMessage = Serializer.Deserialize(data);

        switch (clientMessage.MessageCase)
        {
            case ClientMessage.MessageOneofCase.SendChat:
                _logger.LogInformation("Received chat message: {0}", clientMessage.SendChat.Text);
                var playerId = connection.Id.ToString();
                var sendChatMessage = new ServerMessage
                {
                    Chat = new()
                    {
                        Player = new()
                        {
                            Id = playerId
                        },
                        Text = clientMessage.SendChat.Text
                    }
                };

                await server.WriteToAllConnections(Serializer.Serialize(sendChatMessage));
                break;
            case ClientMessage.MessageOneofCase.Position:
                var playerPositionMessage = new ServerMessage
                {
                    PlayerPosition = new()
                    {
                        Player = new()
                        {
                            Id = connection.Id.ToString()
                        },
                        Position = clientMessage.Position
                    }
                };
                await server.WriteToConnections(Serializer.Serialize(playerPositionMessage), connection.Id);
                break;
            case ClientMessage.MessageOneofCase.JoinGame:
                var playerJoinedMessage = new ServerMessage
                {
                    PlayerJoined = new()
                    {
                        Player = clientMessage.JoinGame.Player
                    }
                };
                var playerPositions = _playerGameState.Players.Select(p => new PlayerPosition
                {
                    Player = new()
                    {
                        Id = p.Id.ToString(),
                        Name = p.Name
                    },
                    Position = new()
                    {
                        X = p.PositionX,
                        Y = p.PositionY
                    }
                });
                var gameStateMessage = new ServerMessage { GameState = new() };
                gameStateMessage.GameState.PlayerPositions.AddRange(playerPositions);

                await server.WriteToAllConnections(Serializer.Serialize(playerJoinedMessage));
                await connection.Write(Serializer.Serialize(gameStateMessage));
                break;
        }
    }
}
