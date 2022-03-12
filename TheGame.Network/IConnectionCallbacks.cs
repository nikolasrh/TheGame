namespace TheGame.Network;

public interface IConnectionCallbacks
{
    public Task OnDisconnect(Connection connection);
    public Task OnRead(byte[] data, Connection connection);
}
