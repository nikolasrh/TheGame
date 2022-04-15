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
        while (!_connection.Disconnected)
        {
            var message = Console.ReadLine() ?? string.Empty;

            var serializedClientMessage = Serializer.Serialize(new ClientMessage
            {
                SendChat = new SendChat
                {
                    Text = message
                }
            });

            await _connection.Write(serializedClientMessage);
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
                _logger.LogInformation("{playerId}: {text}", chat.Player.Id, chat.Text);
                break;
            case ServerMessage.MessageOneofCase.PlayerJoined:
                var playerJoined = serverMessage.PlayerJoined;
                _logger.LogInformation("Player {playerId} joined", playerJoined.Player.Id);
                break;
            case ServerMessage.MessageOneofCase.PlayerLeft:
                var playerLeft = serverMessage.PlayerLeft;
                _logger.LogInformation("Player {playerId} left", playerLeft.Player.Id);
                break;
        }
    }
}
