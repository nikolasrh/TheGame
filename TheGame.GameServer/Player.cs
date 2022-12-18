namespace TheGame.GameServer;

public readonly record struct Player(Guid ConnectionId, string Name);
