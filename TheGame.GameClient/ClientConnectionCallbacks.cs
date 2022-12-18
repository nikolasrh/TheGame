using System.Collections.Concurrent;

using Microsoft.Extensions.Logging;

using TheGame.NetworkConnection;
using TheGame.Protobuf;

namespace TheGame.GameClient;

public class ClientConnectionCallbacks : IConnectionCallbacks<ServerMessage>
{
    private readonly ILogger _logger;
    private readonly ConcurrentDictionary<Guid, Protobuf.Player> _players = new();

    public ClientConnectionCallbacks(ILogger<ClientConnectionCallbacks> logger)
    {
        _logger = logger;
    }

    public void OnDisconnect()
    {
        _logger.LogInformation("Lost connection to server");
    }

    public void OnMessage(ServerMessage message)
    {
        switch (message.MessageCase)
        {
            case ServerMessage.MessageOneofCase.Chat:
                var chat = message.Chat;
                _logger.LogInformation("{player}: {text}", chat.Player.Name, chat.Text);
                break;
            case ServerMessage.MessageOneofCase.PlayerJoined:
                var playerJoined = message.PlayerJoined;
                _logger.LogInformation("{player} connected", playerJoined.Player.Name);
                break;
            case ServerMessage.MessageOneofCase.PlayerLeft:
                var playerLeft = message.PlayerLeft;
                _logger.LogInformation("{player} disconnected", playerLeft.Player.Name);
                break;
            case ServerMessage.MessageOneofCase.SyncPlayers:
                foreach (var player in message.SyncPlayers.Players)
                {
                    _players.AddOrUpdate(Guid.Parse(player.Id), _id => player, (_id, _player) => player);
                }
                break;
        }
    }
}
