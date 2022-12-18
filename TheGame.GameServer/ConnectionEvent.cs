namespace TheGame.GameServer;

public readonly record struct ConnectionEvent(Guid connectionId, ConnectionEventType type);
