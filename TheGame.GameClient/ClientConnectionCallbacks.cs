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
                HandleChat(message.Chat);
                break;
            case ServerMessage.MessageOneofCase.PlayerJoined:
                HandlePlayerJoined(message.PlayerJoined);
                break;
            case ServerMessage.MessageOneofCase.PlayerLeft:
                HandlePlayerLeft(message.PlayerLeft);
                break;
            case ServerMessage.MessageOneofCase.SyncPlayers:
                HandleSyncPlayers(message.SyncPlayers);
                break;
            case ServerMessage.MessageOneofCase.PlayerUpdated:
                HandlePlayerUpdated(message.PlayerUpdated);
                break;
        }
    }

    private void HandleChat(Chat chat)
    {
        _logger.LogInformation("{player}: {text}", chat.Player.Name, chat.Text);
    }

    private void HandlePlayerJoined(PlayerJoined playerJoined)
    {
        var player = playerJoined.Player;

        if (_players.TryAdd(Guid.Parse(player.Id), player))
        {
            _logger.LogInformation("{player} connected", player.Name);
        }
    }

    private void HandlePlayerLeft(PlayerLeft playerLeft)
    {
        _logger.LogInformation("{player} disconnected", playerLeft.Player.Name);
    }

    private void HandleSyncPlayers(SyncPlayers syncPlayers)
    {
        foreach (var player in syncPlayers.Players)
        {
            _players.AddOrUpdate(Guid.Parse(player.Id), _ => player, (_, _) => player);
        }
    }

    private void HandlePlayerUpdated(PlayerUpdated playerUpdated)
    {
        var player = playerUpdated.Player;
        var connectionId = Guid.Parse(player.Id);

        if (_players.TryGetValue(connectionId, out var oldPlayer))
        {
            if (_players.TryUpdate(connectionId, player, oldPlayer))
            {
                _logger.LogInformation("{oldPlayer} changed name to {player}", oldPlayer.Name, player.Name);
            }
        }
    }
}
