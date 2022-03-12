using TheGame.Core;

namespace TheGame.Core;

public interface IConnectionManagerCallbacks
{
    public Task OnConnection(Connection connection, ConnectionManager connectionManager);
    public Task OnDisconnect(ConnectionManager connectionManager);
    public Task OnRead(byte[] data, Connection connection, ConnectionManager connectionManager);
}
