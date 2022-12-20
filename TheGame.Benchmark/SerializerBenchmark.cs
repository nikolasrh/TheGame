using BenchmarkDotNet.Attributes;

using Google.Protobuf;

using TheGame.GameServer;
using TheGame.Protobuf;

namespace TheGame.Benchmark;

[MemoryDiagnoser]
public class SerializerBenchmark
{
    private static ServerMessage? _message;
    private static ServerMessageSerializer? _serializer;
    private static int _messageSize;
    private static byte[]? _serializedMessageArray;

    [GlobalSetup]
    public void GlobalSetup()
    {
        var playerId = Guid.NewGuid().ToString();
        var gameState = new GameState();

        gameState.Players.Add(new Protobuf.Player
        {
            Id = Guid.NewGuid().ToString(),
            Name = playerId
        });

        _message = new ServerMessage
        {
            Welcome = new Welcome
            {
                PlayerId = playerId,
                GameState = gameState
            }
        };

        _serializer = new ServerMessageSerializer();

        _messageSize = _message.CalculateSize();
        _serializedMessageArray = new byte[_messageSize];
        _serializer.SerializeOutgoingMessage(_message, _serializedMessageArray);
    }

    [Benchmark]
    public void Serialize_Serializer()
    {
        var messageSize = _message!.CalculateSize();
        Span<byte> buffer = stackalloc byte[messageSize];
        _serializer!.SerializeOutgoingMessage(_message, buffer);
    }

    [Benchmark]
    public void Serialize_WriteTo()
    {
        var messageSize = _message!.CalculateSize();
        Span<byte> buffer = stackalloc byte[messageSize];
        _message.WriteTo(buffer);
    }

    [Benchmark]
    public void Serialize_ToByteArray()
    {
        _message.ToByteArray();
    }

    [Benchmark]
    public void Deserialize_Deserializer()
    {
        _serializer!.DeserializeIncomingMessage(_serializedMessageArray);
    }

    [Benchmark]
    public void Deserialize_ParseFromArray()
    {
        ServerMessage.Parser.ParseFrom(_serializedMessageArray);
    }

    [Benchmark]
    public void Deserialize_ParseFromSpan()
    {
        Span<byte> span = stackalloc byte[_messageSize];
        _serializedMessageArray.CopyTo(span);

        ServerMessage.Parser.ParseFrom(span);
    }
}
