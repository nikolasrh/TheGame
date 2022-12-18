using Google.Protobuf;

using TheGame.NetworkConnection;
using TheGame.Protobuf;

namespace TheGame.GameServer;

class ServerMessageSerializer : IMessageSerializer<ClientMessage, ServerMessage>
{
    public ClientMessage DeserializeIncomingMessage(ReadOnlySpan<byte> message)
    {
        return ClientMessage.Parser.ParseFrom(message);
    }

    public ReadOnlySpan<byte> SerializeOutgoingMessage(ServerMessage message)
    {
        return message.ToByteArray();
    }
}
