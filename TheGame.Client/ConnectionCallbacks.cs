using Microsoft.Extensions.Logging;

using TheGame.Network;

namespace TheGame.Client;

public class ConnectionCallbacks : IConnectionCallbacks
{
    private readonly ILogger<ConnectionCallbacks> _logger;

    public ConnectionCallbacks(ILogger<ConnectionCallbacks> logger)
    {
        _logger = logger;
    }

    public void OnDisconnect(Connection connection)
    {
        _logger.LogInformation("Lost connection to server");
    }
}
