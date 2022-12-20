using Google.Protobuf;

using TheGame.NetworkConnection;
using TheGame.Protobuf;

namespace TheGame.GameServer;

public class ServerMessageSerializer : IMessageSerializer<ClientMessage, ServerMessage>
{
    public int CalculateMessageSize(ServerMessage message)
    {
        return message.CalculateSize();
    }

    public ClientMessage DeserializeIncomingMessage(ReadOnlySpan<byte> message)
    {
        return ClientMessage.Parser.ParseFrom(message);
    }

    public void SerializeOutgoingMessage(ServerMessage message, Span<byte> buffer)
    {
        message.WriteTo(buffer);
    }
}
