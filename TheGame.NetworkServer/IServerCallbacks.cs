namespace TheGame.NetworkServer;

public interface IServerCallbacks<TIncomingMessage>
{
    public void OnConnection(Guid connectionId);
    public void OnMessage(Guid connectionId, TIncomingMessage message);
    public void OnDisconnect(Guid connectionId);
}
