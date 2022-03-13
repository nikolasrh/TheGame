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
        _server.Disconnect(connection);

        return _serverCallbacks.OnDisconnect(connection, _server);
    }

    public Task OnRead(byte[] data, Connection connection)
    {
        return _serverCallbacks.OnRead(data, connection, _server);
    }
}
