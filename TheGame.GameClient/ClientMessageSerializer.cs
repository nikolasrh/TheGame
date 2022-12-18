using System.Runtime.InteropServices;

using Google.Protobuf;

using TheGame.NetworkConnection;
using TheGame.Protobuf;

namespace TheGame.GameClient;

class ClientMessageSerializer : IMessageSerializer<ServerMessage, ClientMessage>
{
    public ServerMessage DeserializeIncomingMessage(ReadOnlySpan<byte> message)
    {
        return ServerMessage.Parser.ParseFrom(message);
    }

    public ReadOnlySpan<byte> SerializeOutgoingMessage(ClientMessage message)
    {
        return message.ToByteArray();
    }
}
