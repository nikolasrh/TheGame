namespace TheGame.Network;

public interface IServerCallbacks
{
    public Task OnConnection(Connection connection, Server server);
    public Task OnDisconnect(Server server);
    public Task OnRead(byte[] data, Connection connection, Server server);
}
