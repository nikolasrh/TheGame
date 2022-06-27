using TheGame.Network;

namespace TheGame.DedicatedServer;

public class GameServerCallbacks : IServerCallbacks
{
    private readonly GameEventHandler _gameEventHandler;

    public GameServerCallbacks(GameEventHandler clientMessageHandler)
    {
        _gameEventHandler = clientMessageHandler;
    }

    public void OnConnection(Connection connection)
    {
    }

    public void OnConnectionRead(Guid connectionId, byte[] data)
    {
        _gameEventHandler.HandleClientMessage(connectionId, data);
    }

    public void OnDisconnect(Guid connectionId)
    {
        _gameEventHandler.HandleDisconnect(connectionId);
    }
}
