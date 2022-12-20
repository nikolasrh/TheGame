using Microsoft.Extensions.Logging;

using TheGame.Common;
using TheGame.NetworkConnection;
using TheGame.Protobuf;

namespace TheGame.GameClient;

public class Game
{
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
}
