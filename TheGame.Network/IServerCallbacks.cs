namespace TheGame.Network;

public interface IServerCallbacks
{
    public Task OnConnection(Connection connection);
    public void OnDisconnect(Guid connectionId);
}
