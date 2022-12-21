using System.Collections.Concurrent;

using Microsoft.Extensions.Logging;

using TheGame.Common;
using TheGame.NetworkConnection;
using TheGame.Protobuf;

namespace TheGame.GameClient;

public class Game
{
    private Guid PlayerId { get; set; }
    private readonly ConcurrentDictionary<Guid, Protobuf.Player> _players = new();
    private readonly Connection<ServerMessage, ClientMessage> _connection;
    private readonly ILogger _logger;
    private string PlayerName { get; set; } = string.Empty;
    private bool Exiting { get; set; } = false;

    public Game(Connection<ServerMessage, ClientMessage> connection, ILogger<Game> logger)
    {
        _connection = connection;
        _logger = logger;
    }

    public void Start(Loop loop)
    {
        _connection.Disconnected += () => Exiting = true;
        _connection.MessageReceived += MessageHandler;

        Console.Write("Name: ");
        PlayerName = Console.ReadLine() ?? string.Empty;

        SendJoinGameMessage();

        var gameThread = new Thread(() =>
        {
            loop.Run(_ => _connection.HandleIncomingMessages());
        });
        gameThread.Start();

        while (true)
        {
            var message = Console.ReadLine() ?? string.Empty;

            const string EXIT_COMMAND = "/exit";
            const string NAME_COMMAND = "/name ";

            if (message == EXIT_COMMAND)
            {
                SendLeaveGameMessage();
                loop.Exit();
                _connection.Disconnect();
                break;
            }
            if (message.StartsWith(NAME_COMMAND))
            {
                const int NAME_COMMAND_LENGTH = 6;
                var name = message.Substring(NAME_COMMAND_LENGTH);
                SendChangeNameMessage(name);
            }
            else
            {
                SendChatMessage(message);
            }
        }
    }

    private void SendJoinGameMessage()
    {
        var joinGame = new ClientMessage
        {
            JoinGame = new JoinGame
            {
                Name = PlayerName
            }
        };

        SendMessage(joinGame);
    }

    private void SendLeaveGameMessage()
    {
        var leaveGame = new ClientMessage
        {
            LeaveGame = new LeaveGame()
        };

        SendMessage(leaveGame);
    }

    private void SendChatMessage(string message)
    {
        var sendChat = new ClientMessage
        {
            SendChat = new SendChat
            {
                Text = message
            }
        };

        SendMessage(sendChat);
    }

    private void SendChangeNameMessage(string name)
    {
        var changeName = new ClientMessage
        {
            ChangeName = new ChangeName
            {
                Name = name
            }
        };

        SendMessage(changeName);
    }

    private void SendMessage(ClientMessage message)
    {
        _connection.QueueMessage(message);
        _connection.SendQueuedMessages();
    }

    public void MessageHandler(ServerMessage message)
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
        }
    }

    private void HandleChat(Chat chat)
    {
        if (_players.TryGetValue(Guid.Parse(chat.PlayerId), out var player))
        {
            _logger.LogInformation("{player}: {text}", player.Name, chat.Text);
        }
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
        if (_players.TryGetValue(Guid.Parse(playerLeft.PlayerId), out var player))
        {
            _logger.LogInformation("{player} disconnected", player.Name);
        }
    }

    private void HandleWelcome(Welcome welcome)
    {
        PlayerId = Guid.Parse(welcome.PlayerId);

        foreach (var player in welcome.GameState.Players)
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
