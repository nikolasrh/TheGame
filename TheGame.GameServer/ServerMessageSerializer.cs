using Google.Protobuf;

using TheGame.NetworkConnection;
using TheGame.Protobuf;

namespace TheGame.GameServer;

class ServerMessageSerializer : IMessageSerializer<ClientMessage, ServerMessage>
{
    public ClientMessage DeserializeIncomingMessage(byte[] message)
    {
        return ClientMessage.Parser.ParseFrom(message);
    }

    public byte[] SerializeOutgoingMessage(ServerMessage message)
    {
        return message.ToByteArray();
    }
}
