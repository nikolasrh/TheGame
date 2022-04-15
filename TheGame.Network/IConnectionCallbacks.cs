namespace TheGame.Network;

public interface IConnectionCallbacks
{
    public void OnDisconnect(Connection connection);
}
