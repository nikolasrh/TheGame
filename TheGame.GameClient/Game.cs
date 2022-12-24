using System.Collections.Concurrent;

using Microsoft.Extensions.Logging;

using TheGame.Common;
using TheGame.NetworkConnection;
using TheGame.Protobuf;

namespace TheGame.GameClient;

public class Game
{
    private readonly ConcurrentDictionary<Guid, Protobuf.Player> _players = new();
    private readonly Loop _loop;
    private readonly Connection<ServerMessage, ClientMessage> _connection;
    private readonly ILogger _logger;
    private readonly Thread _backgroundThread;

    public Game(Loop loop, Connection<ServerMessage, ClientMessage> connection, ILogger<Game> logger)
    {
        _loop = loop;
        _connection = connection;
        _logger = logger;
        _connection.MessageReceived += MessageHandler;

        _backgroundThread = new Thread(() =>
        {
            _loop.Run(_ => _connection.HandleIncomingMessages());
        });
        _backgroundThread.IsBackground = true;
    }

    public delegate void ChatMessageReceivedEventHandler(Player player, string message);
    public delegate void PlayerJoinedEventHandler(Player player);
    public delegate void PlayerLeftEventHandler(Player player);
    public delegate void PlayerUpdatedEventHandler(Player oldPlayer, Player newPlayer);
    public delegate void PlayerMovedEventHandler(Player player, float x, float y);
    public delegate void GameJoinedEventHandler(string playerId, IEnumerable<Player> players);
    public event ChatMessageReceivedEventHandler? ChatMesageReceived;
    public event PlayerJoinedEventHandler? PlayerJoined;
    public event PlayerLeftEventHandler? PlayerLeft;
    public event PlayerUpdatedEventHandler? PlayerUpdated;
    public event PlayerMovedEventHandler? PlayerMoved;
    public event GameJoinedEventHandler? GameJoined;

    public bool Connected { get => _connection.Connected; }

    public bool Connect(string hostname, int port)
    {
        if (!_connection.Connect(hostname, port)) return false;

        if (_backgroundThread.ThreadState.HasFlag(ThreadState.Unstarted))
        {
            _backgroundThread.Start();
        }

        return true;
    }

    public void Disconnect()
    {
        var leaveGame = new ClientMessage
        {
            LeaveGame = new LeaveGame()
        };

        SendClientMessage(leaveGame);

        _connection.Disconnect();
    }

    public void JoinGame(string name)
    {
        var joinGame = new ClientMessage
        {
            JoinGame = new JoinGame
            {
                Name = name
            }
        };

        SendClientMessage(joinGame);
    }

    public void SendChatMessage(string message)
    {
        var sendChat = new ClientMessage
        {
            SendChat = new SendChat
            {
                Text = message
            }
        };

        SendClientMessage(sendChat);
    }

    public void ChangeName(string name)
    {
        var changeName = new ClientMessage
        {
            ChangeName = new ChangeName
            {
                Name = name
            }
        };

        SendClientMessage(changeName);
    }

    public void Move(float x, float y)
    {
        var changeName = new ClientMessage
        {
            Move = new Move
            {
                X = x,
                Y = y
            }
        };

        SendClientMessage(changeName);
    }

    private void SendClientMessage(ClientMessage message)
    {
        _connection.QueueMessage(message);
        _connection.SendQueuedMessages();
    }

    private void MessageHandler(ServerMessage message)
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
            case ServerMessage.MessageOneofCase.Welcome:
                HandleWelcome(message.Welcome);
                break;
            case ServerMessage.MessageOneofCase.PlayerUpdated:
                HandlePlayerUpdated(message.PlayerUpdated);
                break;
            case ServerMessage.MessageOneofCase.PlayerMoved:
                HandlePlayerMoved(message.PlayerMoved);
                break;
        }
    }

    private void HandleChat(Protobuf.Chat chat)
    {
        if (_players.TryGetValue(Guid.Parse(chat.PlayerId), out var player))
        {
            ChatMesageReceived?.Invoke(player, chat.Text);
        }
    }

    private void HandlePlayerJoined(PlayerJoined playerJoined)
    {
        var player = playerJoined.Player;

        if (_players.TryAdd(Guid.Parse(player.Id), player))
        {
            PlayerJoined?.Invoke(player);
        }
    }

    private void HandlePlayerLeft(PlayerLeft playerLeft)
    {
        if (_players.TryGetValue(Guid.Parse(playerLeft.PlayerId), out var player))
        {
            PlayerLeft?.Invoke(player);
        }
    }

    private void HandleWelcome(Welcome welcome)
    {
        foreach (var player in welcome.GameState.Players)
        {
            _players.AddOrUpdate(Guid.Parse(player.Id), _ => player, (_, _) => player);
        }

        GameJoined?.Invoke(welcome.PlayerId.ToString(), welcome.GameState.Players);
    }

    private void HandlePlayerUpdated(PlayerUpdated playerUpdated)
    {
        var newPlayer = playerUpdated.Player;
        var connectionId = Guid.Parse(newPlayer.Id);

        if (_players.TryGetValue(connectionId, out var oldPlayer))
        {
            if (_players.TryUpdate(connectionId, newPlayer, oldPlayer))
            {
                PlayerUpdated?.Invoke(oldPlayer, newPlayer);
            }
        }
    }

    private void HandlePlayerMoved(PlayerMoved playerMoved)
    {
        var playerId = Guid.Parse(playerMoved.PlayerId);

        if (_players.TryGetValue(playerId, out var player))
        {
            PlayerMoved?.Invoke(player, playerMoved.X, playerMoved.Y);
        }
    }
}
