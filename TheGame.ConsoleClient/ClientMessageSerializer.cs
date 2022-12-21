using System.Runtime.InteropServices;

using Google.Protobuf;

using TheGame.NetworkConnection;
using TheGame.Protobuf;

namespace TheGame.ConsoleClient;

public class ClientMessageSerializer : IMessageSerializer<ServerMessage, ClientMessage>
{
    public int CalculateMessageSize(ClientMessage message)
    {
        return message.CalculateSize();
    }

    public ServerMessage DeserializeIncomingMessage(ReadOnlySpan<byte> message)
    {
        return ServerMessage.Parser.ParseFrom(message);
    }

    public void SerializeOutgoingMessage(ClientMessage message, Span<byte> buffer)
    {
        message.WriteTo(buffer);
    }
}
