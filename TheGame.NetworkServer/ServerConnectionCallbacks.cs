using TheGame.NetworkConnection;

namespace TheGame.NetworkServer;

public class ServerConnectionCallbacks<TIncomingMessage> : IConnectionCallbacks<TIncomingMessage>
{
    private readonly Guid _connectionId;
    private readonly IServerCallbacks<TIncomingMessage> _serverCallbacks;

    public ServerConnectionCallbacks(Guid connectionId, IServerCallbacks<TIncomingMessage> serverCallbacks)
    {
        _connectionId = connectionId;
        _serverCallbacks = serverCallbacks;
    }

    public void OnMessage(TIncomingMessage message)
    {
        _serverCallbacks.OnMessage(_connectionId, message);
    }

    public void OnDisconnect()
    {
        _serverCallbacks.OnDisconnect(_connectionId);
    }
}
