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

        while (!_connection.Disconnected)
        {
            var message = Console.ReadLine() ?? string.Empty;

            if (message == "/exit")
            {
                SendLeaveGameMessage();
                loop.Exit();
                _connection.Disconnect();
                break;
            }

            SendChatMessage(message);
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

        _connection.SendMessage(joinGame);
    }

    private void SendLeaveGameMessage()
    {
        var leaveGame = new ClientMessage
        {
            LeaveGame = new LeaveGame()
        };

        _connection.SendMessage(leaveGame);
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

        _connection.SendMessage(sendChat);
    }
}
