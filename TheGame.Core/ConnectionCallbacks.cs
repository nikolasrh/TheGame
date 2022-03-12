namespace TheGame.Core;

public class ConnectionCallbacks : IConnectionCallbacks
{
    private readonly ConnectionManager _connectionManager;
    private readonly IConnectionManagerCallbacks _connectionManagerCallbacks;

    public ConnectionCallbacks(ConnectionManager connectionManager, IConnectionManagerCallbacks connectionManagerCallbacks)
    {
        _connectionManager = connectionManager;
        _connectionManagerCallbacks = connectionManagerCallbacks;
    }

    public Task OnRead(byte[] data, Connection connection)
    {
        return _connectionManagerCallbacks.OnRead(data, connection, _connectionManager);
    }
}
