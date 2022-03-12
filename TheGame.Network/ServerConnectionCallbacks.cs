namespace TheGame.Network;

public class ServerConnectionCallbacks : IConnectionCallbacks
{
    private readonly Server _server;
    private readonly IServerCallbacks _serverCallbacks;

    public ServerConnectionCallbacks(Server server, IServerCallbacks serverCallbacks)
    {
        _server = server;
        _serverCallbacks = serverCallbacks;
    }

    public Task OnDisconnect(Connection connection)
    {
        return Task.CompletedTask;
    }

    public Task OnRead(byte[] data, Connection connection)
    {
        return _serverCallbacks.OnRead(data, connection, _server);
    }
}
