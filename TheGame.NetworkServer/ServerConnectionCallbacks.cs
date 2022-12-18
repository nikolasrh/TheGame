using Microsoft.Extensions.Logging;

using TheGame.NetworkConnection;

namespace TheGame.NetworkServer;

public class ServerConnectionCallbacks<TIncomingMessage> : IConnectionCallbacks<TIncomingMessage>
{
    private readonly Guid _connectionId;
    private readonly IServerCallbacks<TIncomingMessage> _serverCallbacks;
    private readonly ILogger _logger;

    public ServerConnectionCallbacks(
        Guid connectionId,
        IServerCallbacks<TIncomingMessage> serverCallbacks,
        ILogger<ServerConnectionCallbacks<TIncomingMessage>> logger)
    {
        _connectionId = connectionId;
        _serverCallbacks = serverCallbacks;
        _logger = logger;
    }

    public void OnMessage(TIncomingMessage message)
    {
        _serverCallbacks.OnMessage(_connectionId, message);
    }

    public void OnDisconnect()
    {
        _logger.LogInformation("Stopped connection {0}", _connectionId);
        _serverCallbacks.OnDisconnect(_connectionId);
    }
}
