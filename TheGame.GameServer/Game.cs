using System.Collections.Concurrent;

using Microsoft.Extensions.Logging;

using TheGame.Common;
using TheGame.NetworkServer;
using TheGame.Protobuf;

namespace TheGame.GameServer;

public class Game
{
    private readonly ConcurrentDictionary<Guid, NewConnection> _newConnections = new();
    private readonly ConcurrentDictionary<Guid, Player> _players = new();
    private readonly ConcurrentQueue<ClientMessageEvent> _clientMessageEventQueue;
    private readonly ConcurrentQueue<ConnectionEvent> _connectionEventQueue;
    private readonly Server<ClientMessage, ServerMessage> _server;
    private readonly ILogger _logger;
    private readonly GameEventHandler _gameEventHandler;

    public Game(
        ConcurrentQueue<ClientMessageEvent> clientMessageEventQueue,
        ConcurrentQueue<ConnectionEvent> connectionEventQueue,
        Server<ClientMessage, ServerMessage> server,
        ILogger<Game> logger)
    {
        _clientMessageEventQueue = clientMessageEventQueue;
        _connectionEventQueue = connectionEventQueue;
        _server = server;
        _logger = logger;
        _gameEventHandler = new GameEventHandler(this, server);
    }

    public void Start(Loop loop)
    {
        var gameThread = new Thread(() =>
        {
            loop.Run(_ =>
            {
                while (_connectionEventQueue.TryDequeue(out ConnectionEvent connectionEvent))
                {
                    _gameEventHandler.HandleConnectionEvent(connectionEvent);
                }
                while (_clientMessageEventQueue.TryDequeue(out ClientMessageEvent clientMessageEvent))
                {
                    _gameEventHandler.HandleClientMessageEvent(clientMessageEvent);
                }
            });
        });
        gameThread.Start();
    }

    public void AddNewConnection(Guid connectionId)
    {
        _newConnections.TryAdd(connectionId, new NewConnection(connectionId, DateTime.Now));
    }

    public NewConnection? RemoveNewConnection(Guid id)
    {
        _newConnections.Remove(id, out var newConnection);
        return newConnection;
    }

    public void AddPlayer(Player player)
    {
        _players.TryAdd(player.Id, player);
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
