using Google.Protobuf;

namespace TheGame.Core.Serialization;

public static class Serializer
{
    public static byte[] Serialize(GameEvent data)
    {
        return data.ToByteArray();
    }

    public static GameEvent Deserialize(byte[] bytes)
    {
        return GameEvent.Parser.ParseFrom(bytes);
    }
}
