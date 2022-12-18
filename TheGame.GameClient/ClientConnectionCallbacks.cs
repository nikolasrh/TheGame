using Microsoft.Extensions.Logging;

using TheGame.NetworkConnection;
using TheGame.Protobuf;

namespace TheGame.GameClient;

public class ClientConnectionCallbacks : IConnectionCallbacks<ServerMessage>
{
    private readonly ILogger _logger;

    public ClientConnectionCallbacks(ILogger<ClientConnectionCallbacks> logger)
    {
        _logger = logger;
    }

    public void OnDisconnect()
    {
        _logger.LogInformation("Lost connection to server");
    }

    public void OnMessage(ServerMessage message)
    {
        switch (message.MessageCase)
        {
            case ServerMessage.MessageOneofCase.Chat:
                var chat = message.Chat;
                _logger.LogInformation("{player}: {text}", chat.Player.Name, chat.Text);
                break;
            case ServerMessage.MessageOneofCase.PlayerJoined:
                var playerJoined = message.PlayerJoined;
                _logger.LogInformation("{player} connected", playerJoined.Player.Name);
                break;
            case ServerMessage.MessageOneofCase.PlayerLeft:
                var playerLeft = message.PlayerLeft;
                _logger.LogInformation("{player} disconnected", playerLeft.Player.Name);
                break;
        }
    }
}
