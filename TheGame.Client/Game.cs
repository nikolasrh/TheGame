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

    public async Task Run()
    {
        await Task.WhenAll(_connection.Start(), StartReading(), StartWriting());
    }

    private async Task StartWriting()
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

        await _connection.Write(joinGame);

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

            await _connection.Write(sendChat);
        }
    }

    private async Task StartReading()
    {
        byte[]? data;
        while ((data = await _connection.Read()) != null)
        {
            var serverMessage = Serializer.Deserialize(data);
            HandleServerMessage(serverMessage);
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
