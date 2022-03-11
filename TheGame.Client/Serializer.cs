using Google.Protobuf;

using TheGame.Protobuf;

namespace TheGame.Client;

public static class Serializer
{
    public static byte[] Serialize(ClientMessage data)
    {
        return data.ToByteArray();
    }

    public static ServerMessage Deserialize(byte[] bytes)
    {
        return ServerMessage.Parser.ParseFrom(bytes);
    }
}
