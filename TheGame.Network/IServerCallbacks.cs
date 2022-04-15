namespace TheGame.Network;

public interface IServerCallbacks
{
    public Task OnBeforeConnection(Connection connection, Server server);
    public Task OnAfterConnection(Connection connection, Server server);
    public Task OnDisconnect(Connection connection, Server server);
    public Task OnRead(byte[] data, Connection connection, Server server);
}
