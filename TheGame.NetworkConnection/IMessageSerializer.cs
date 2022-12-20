namespace TheGame.NetworkConnection;

public interface IMessageSerializer<TIncomingMessage, TOutgoingMessage>
{
    int CalculateMessageSize(TOutgoingMessage message);
    void SerializeOutgoingMessage(TOutgoingMessage message, Span<byte> buffer);
    TIncomingMessage DeserializeIncomingMessage(ReadOnlySpan<byte> message);
}
