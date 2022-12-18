namespace TheGame.NetworkConnection;

public interface IConnectionCallbacks<TIncomingMessage>
{
    public void OnMessage(TIncomingMessage message);
    public void OnDisconnect();
}
