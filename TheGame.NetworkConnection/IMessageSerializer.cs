namespace TheGame.NetworkConnection;

public interface IMessageSerializer<TIncomingMessage, TOutgoingMessage>
{
    ReadOnlySpan<byte> SerializeOutgoingMessage(TOutgoingMessage message);
    TIncomingMessage DeserializeIncomingMessage(ReadOnlySpan<byte> message);
}
