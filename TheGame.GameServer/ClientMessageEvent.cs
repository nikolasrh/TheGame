using TheGame.Protobuf;

namespace TheGame.GameServer;

public readonly record struct ClientMessageEvent(Guid connectionId, ClientMessage message);
