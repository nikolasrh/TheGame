namespace TheGame.Network;

public interface IServerCallbacks
{
    public void OnConnection(Connection connection);
    public void OnConnectionRead(Guid connectionId, byte[] data);
    public void OnDisconnect(Guid connectionId);
}
