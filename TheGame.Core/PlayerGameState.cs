using System.Collections.Concurrent;

namespace TheGame.Core;

public class PlayerGameState
{
    private readonly ConcurrentDictionary<Guid, Player> _players = new();

    public IEnumerable<Player> Players { get => _players.Select(player => player.Value); }

    public Player? GetPlayer(Guid id)
    {
        if (_players.TryGetValue(id, out var player))
        {
            return player;
        }
        return null;
    }

    public void AddPlayer(Player player)
    {
        _players.AddOrUpdate(player.Id, _key => player, (_key, _player) => player);
    }

    public void UpdatePlayerPosition(Guid playerId, float x, float y)
    {
        if (_players.TryGetValue(playerId, out var player))
        {
            player.PositionX = x;
            player.PositionY = y;
        }
    }

    public void RemovePlayer(Guid playerId)
    {
        _players.TryRemove(playerId, out _);
    }
}
