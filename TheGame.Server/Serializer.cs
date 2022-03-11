using Google.Protobuf;

using TheGame.Protobuf;

namespace TheGame.Server;

public static class Serializer
{
    public static byte[] Serialize(ServerMessage message)
    {
        return message.ToByteArray();
    }

    public static ClientMessage Deserialize(byte[] bytes)
    {
        return ClientMessage.Parser.ParseFrom(bytes);
    }
}
