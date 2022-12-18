
using Microsoft.Extensions.Logging;

namespace TheGame.NetworkServer;

public class ServerConnectionCallbacksFactory<TIncomingMessage>
{
    private readonly IServerCallbacks<TIncomingMessage> _serverCallbacks;
    private readonly ILogger<ServerConnectionCallbacks<TIncomingMessage>> _logger;

    public ServerConnectionCallbacksFactory(
        IServerCallbacks<TIncomingMessage> serverCallbacks,
        ILogger<ServerConnectionCallbacks<TIncomingMessage>> logger)
    {
        _serverCallbacks = serverCallbacks;
        _logger = logger;
    }

    public ServerConnectionCallbacks<TIncomingMessage> Create(Guid connectionId)
    {
        return new ServerConnectionCallbacks<TIncomingMessage>(connectionId, _serverCallbacks, _logger);
    }
}
