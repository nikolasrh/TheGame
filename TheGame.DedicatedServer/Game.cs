using System.Collections.Concurrent;

using Microsoft.Extensions.Logging;

using TheGame.Network;

namespace TheGame.DedicatedServer;

public class Game
{
    private readonly ConcurrentDictionary<Guid, Player> _players = new();
    private readonly ServerMessageQueue _serverMessageQueue;
    private readonly ILogger<Game> _logger;

    public Game(ServerMessageQueue serverMessageQueue, ILogger<Game> logger)
    {
        _serverMessageQueue = serverMessageQueue;
        _logger = logger;
    }

    public async Task Run(Server server)
    {
        while (true)
        {
            _logger.LogInformation("Game waiting to read...");
            var serverMessage = await _serverMessageQueue.Read();
            _logger.LogInformation("Game read {0}", serverMessage);
            var data = Serializer.Serialize(serverMessage);
            await server.WriteAll(data);
        }
    }

    public void AddPlayer(Player player)
    {
        _players.AddOrUpdate(player.Id, _id => player, (_id, _player) => player);
    }

    public Player? RemovePlayer(Guid id)
    {
        _players.Remove(id, out var player);
        return player;
    }

    public Player? GetPlayer(Guid id)
    {
        _players.TryGetValue(id, out var player);
        return player;
    }

    public Player[] GetPlayers()
    {
        return _players.Select(x => x.Value).ToArray();
    }
}
