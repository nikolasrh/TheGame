using Google.Protobuf;

using TheGame.Core.Protobuf;

namespace TheGame.Client.Serialization;

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
