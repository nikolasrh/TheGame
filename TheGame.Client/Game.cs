using Microsoft.Extensions.Logging;

using TheGame.Network;
using TheGame.Protobuf;

namespace TheGame.Client;

public class Game
{
    private readonly Connection _connection;
    private readonly ILogger<Game> _logger;

    public Game(Connection connection, ILogger<Game> logger)
    {
        _connection = connection;
        _logger = logger;
    }

    public void Run()
    {
        var readConnectionThread = new Thread(ReadConnection);
        readConnectionThread.Start();

        var flushConnectionThread = new Thread(FlushConnection);
        flushConnectionThread.Start();

        StartChat();
    }

    private void StartChat()
    {
        Console.Write("Name: ");
        var name = Console.ReadLine() ?? string.Empty;

        var joinGame = Serializer.Serialize(new ClientMessage
        {
            JoinGame = new JoinGame
            {
                Name = name
            }
        });

        _connection.Write(joinGame);

        while (!_connection.Disconnected)
        {
            var message = Console.ReadLine() ?? string.Empty;

            var sendChat = Serializer.Serialize(new ClientMessage
            {
                SendChat = new SendChat
                {
                    Text = message
                }
            });

            _connection.Write(sendChat);
        }
    }

    private void FlushConnection()
    {
        try
        {
            while (true)
            {
                _connection.Flush();
            }
        }
        catch (Exception e)
        {
            _logger.LogInformation(e, $"Stopping {nameof(FlushConnection)}");
        }
    }

    private void ReadConnection()
    {
        try
        {
            while (true)
            {
                byte[]? data;
                while ((data = _connection.Read()) != null)
                {
                    var serverMessage = Serializer.Deserialize(data);
                    HandleServerMessage(serverMessage);
                }
            }
        }
        catch (Exception e)
        {
            _logger.LogInformation(e, $"Stopping {nameof(ReadConnection)}");
        }
    }

    private void HandleServerMessage(ServerMessage serverMessage)
    {
        switch (serverMessage.MessageCase)
        {
            case ServerMessage.MessageOneofCase.Chat:
                var chat = serverMessage.Chat;
                _logger.LogInformation("{player}: {text}", chat.Player.Name, chat.Text);
                break;
            case ServerMessage.MessageOneofCase.PlayerJoined:
                var playerJoined = serverMessage.PlayerJoined;
                _logger.LogInformation("{player} joined", playerJoined.Player.Name);
                break;
            case ServerMessage.MessageOneofCase.PlayerLeft:
                var playerLeft = serverMessage.PlayerLeft;
                _logger.LogInformation("{player} left", playerLeft.Player.Name);
                break;
        }
    }
}
