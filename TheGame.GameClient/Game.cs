using Microsoft.Extensions.Logging;

using TheGame.Common;
using TheGame.NetworkConnection;
using TheGame.Protobuf;

namespace TheGame.GameClient;

public class Game
{
    private readonly Connection<ServerMessage, ClientMessage> _connection;
    private readonly ILogger _logger;

    public Game(Connection<ServerMessage, ClientMessage> connection, ILogger<Game> logger)
    {
        _connection = connection;
        _logger = logger;
    }

    public void Start(Loop loop)
    {
        Console.Write("Name: ");
        var playerName = Console.ReadLine() ?? string.Empty;

        SendJoinGameMessage(playerName);

        var gameThread = new Thread(() =>
        {
            loop.Run(_ => _connection.HandleIncomingMessages());
        });
        gameThread.Start();

        while (!_connection.Disconnected)
        {
            var message = Console.ReadLine() ?? string.Empty;

            SendChatMessage(message);
        }
    }

    private void SendJoinGameMessage(string playerName)
    {
        var joinGame = new ClientMessage
        {
            JoinGame = new JoinGame
            {
                Name = playerName
            }
        };

        _connection.SendMessage(joinGame);
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
