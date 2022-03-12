namespace TheGame.Network;

public class ConnectionCallbacks : IConnectionCallbacks
{
    private readonly Server _server;
    private readonly IServerCallbacks _serverCallbacks;

    public ConnectionCallbacks(Server server, IServerCallbacks serverCallbacks)
    {
        _server = server;
        _serverCallbacks = serverCallbacks;
    }

    public Task OnRead(byte[] data, Connection connection)
    {
        return _serverCallbacks.OnRead(data, connection, _server);
    }
}
