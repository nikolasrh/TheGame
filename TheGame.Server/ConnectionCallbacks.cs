using TheGame.Core;

namespace TheGame.Server;

public class ConnectionCallbacks : IConnectionCallbacks
{
    private readonly ConnectionManager _connectionManager;
    private readonly ConnectionManagerCallbacks _connectionManagerCallbacks;

    public ConnectionCallbacks(ConnectionManager connectionManager, ConnectionManagerCallbacks connectionManagerCallbacks)
    {
        _connectionManager = connectionManager;
        _connectionManagerCallbacks = connectionManagerCallbacks;
    }

    public Task OnRead(byte[] data, Connection connection)
    {
        return _connectionManagerCallbacks.OnRead(data, connection, _connectionManager);
    }
}
