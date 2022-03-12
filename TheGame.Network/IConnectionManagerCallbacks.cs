using TheGame.Network;

namespace TheGame.Network;

public interface IConnectionManagerCallbacks
{
    public Task OnConnection(Connection connection, ConnectionManager connectionManager);
    public Task OnDisconnect(ConnectionManager connectionManager);
    public Task OnRead(byte[] data, Connection connection, ConnectionManager connectionManager);
}
