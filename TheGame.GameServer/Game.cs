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
        _server.ConnectionOpened += ConnectionOpened;
        _server.ConnectionClosed += ConnectionClosed;
        _server.MessageReceived += MessageReceived;

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
                _server.SendMessages();
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
        _players.TryAdd(player.ConnectionId, player);
    }

    public bool UpdatePlayer(Player player)
    {
        if (_players.TryGetValue(player.ConnectionId, out var oldPlayer))
        {
            return _players.TryUpdate(player.ConnectionId, player, oldPlayer);
        }
        return false;
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

    private void ConnectionOpened(Guid connectionId)
    {
        _connectionEventQueue.Enqueue(new ConnectionEvent(connectionId, ConnectionEventType.CONNECT));
    }

    private void ConnectionClosed(Guid connectionId)
    {
        _connectionEventQueue.Enqueue(new ConnectionEvent(connectionId, ConnectionEventType.DISCONNECT));
    }

    private void MessageReceived(Guid connectionId, ClientMessage message)
    {
        _clientMessageEventQueue.Enqueue(new ClientMessageEvent(connectionId, message));
    }
}
