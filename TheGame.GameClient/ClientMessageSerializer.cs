using Google.Protobuf;

using TheGame.NetworkConnection;
using TheGame.Protobuf;

namespace TheGame.GameClient;

class ClientMessageSerializer : IMessageSerializer<ServerMessage, ClientMessage>
{
    public ServerMessage DeserializeIncomingMessage(byte[] message)
    {
        return ServerMessage.Parser.ParseFrom(message);
    }

    public byte[] SerializeOutgoingMessage(ClientMessage message)
    {
        return message.ToByteArray();
    }
}
