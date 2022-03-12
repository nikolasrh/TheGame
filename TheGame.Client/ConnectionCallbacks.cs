using Microsoft.Extensions.Logging;

using TheGame.Network;
using TheGame.Protobuf;

namespace TheGame.Client;

public class ConnectionCallbacks : IConnectionCallbacks
{
    private readonly ILogger<ConnectionCallbacks> _logger;

    public ConnectionCallbacks(ILogger<ConnectionCallbacks> logger)
    {
        _logger = logger;
    }

    public Task OnRead(byte[] data, Connection connection)
    {
        var serverMessage = Serializer.Deserialize(data);

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

        return Task.CompletedTask;
    }
}
