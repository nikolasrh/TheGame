using System.Collections.Concurrent;

using TheGame.NetworkServer;
using TheGame.Protobuf;

namespace TheGame.GameServer;

public class ServerCallbacks : IServerCallbacks<ClientMessage>
{
    private readonly ConcurrentQueue<ClientMessageEvent> _clientMessageEventQueue;
    private readonly ConcurrentQueue<ConnectionEvent> _connectionEventQueue;

    public ServerCallbacks(
        ConcurrentQueue<ClientMessageEvent> clientMessageEventQueue,
        ConcurrentQueue<ConnectionEvent> connectionEventQueue)
    {
        _clientMessageEventQueue = clientMessageEventQueue;
        _connectionEventQueue = connectionEventQueue;
    }

    public void OnConnection(Guid connectionId)
    {
        _connectionEventQueue.Enqueue(new ConnectionEvent(connectionId, ConnectionEventType.CONNECT));
    }

    public void OnDisconnect(Guid connectionId)
    {
        _connectionEventQueue.Enqueue(new ConnectionEvent(connectionId, ConnectionEventType.DISCONNECT));
    }

    public void OnMessage(Guid connectionId, ClientMessage message)
    {
        _clientMessageEventQueue.Enqueue(new ClientMessageEvent(connectionId, message));
    }
}
