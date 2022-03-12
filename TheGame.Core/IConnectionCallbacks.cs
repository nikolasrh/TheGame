namespace TheGame.Core;

public interface IConnectionCallbacks
{
    public Task OnRead(byte[] data, Connection connection);
}
