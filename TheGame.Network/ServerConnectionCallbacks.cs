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

    public void OnDisconnect(Connection connection)
    {
        _server.Disconnect(connection);
        _serverCallbacks.OnDisconnect(connection, _server);
    }
}
