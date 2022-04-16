using TheGame.Network;

namespace TheGame.DedicatedServer;

public class GameServerCallbacks : IServerCallbacks
{
    private readonly GameEventHandler _gameEventHandler;

    public GameServerCallbacks(GameEventHandler clientMessageHandler)
    {
        _gameEventHandler = clientMessageHandler;
    }

    public async Task OnConnection(Connection connection)
    {
        byte[]? data;
        while ((data = await connection.Read()) != null)
        {
            _gameEventHandler.HandleClientMessage(data, connection.Id);
        }
    }

    public void OnDisconnect(Guid connectionId)
    {
        _gameEventHandler.HandleDisconnect(connectionId);
    }
}
