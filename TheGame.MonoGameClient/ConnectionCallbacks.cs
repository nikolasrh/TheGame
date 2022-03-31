using System;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using TheGame.Core;
using TheGame.Network;
using TheGame.Protobuf;

namespace TheGame.MonoGameClient;

public class ConnectionCallbacks : IConnectionCallbacks
{
    private readonly PlayerGameState _playerGameState;
    private readonly ILogger<ConnectionCallbacks> _logger;

    public ConnectionCallbacks(PlayerGameState playerGameState, ILogger<ConnectionCallbacks> logger)
    {
        _playerGameState = playerGameState;
        _logger = logger;
    }

    public Task OnDisconnect(Connection connection)
    {
        return Task.CompletedTask;
    }

    public Task OnRead(byte[] data, Connection connection)
    {
        var serverMessage = Serializer.Deserialize(data);

        switch (serverMessage.MessageCase)
        {
            case ServerMessage.MessageOneofCase.PlayerJoined:
                var player = serverMessage.PlayerJoined.Player;
                _playerGameState.AddPlayer(new(Guid.Parse(player.Id), player.Name));
                break;
            case ServerMessage.MessageOneofCase.PlayerLeft:
                _playerGameState.RemovePlayer(Guid.Parse(serverMessage.PlayerLeft.Player.Id));
                break;
            case ServerMessage.MessageOneofCase.PlayerPosition:
                _playerGameState.UpdatePlayerPosition(
                    Guid.Parse(serverMessage.PlayerPosition.Player.Id),
                    serverMessage.PlayerPosition.Position.X,
                    serverMessage.PlayerPosition.Position.Y);
                break;
            case ServerMessage.MessageOneofCase.GameState:
                foreach (var playerPosition in serverMessage.GameState.PlayerPositions)
                {
                    _playerGameState.AddPlayer(new Core.Player(
                        Guid.Parse(playerPosition.Player.Id),
                        playerPosition.Player.Name)
                    {
                        PositionX = playerPosition.Position.X,
                        PositionY = playerPosition.Position.Y
                    });
                }
                break;
        }

        return Task.CompletedTask;
    }
}
