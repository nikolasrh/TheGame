namespace TheGame.Network;

public interface IConnectionCallbacks
{
    public Task OnRead(byte[] data, Connection connection);
}
