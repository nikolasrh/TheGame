namespace TheGame.Network;

public class ServerConnectionCallbacks : IConnectionCallbacks
{
    private readonly Server _server;

    public ServerConnectionCallbacks(Server server)
    {
        _server = server;
    }

    public void OnDisconnect(Connection connection)
    {
        _server.Disconnect(connection);
    }
}
