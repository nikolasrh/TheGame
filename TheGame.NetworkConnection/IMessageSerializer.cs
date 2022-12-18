namespace TheGame.NetworkConnection;

public interface IMessageSerializer<TIncomingMessage, TOutgoingMessage>
{
    byte[] SerializeOutgoingMessage(TOutgoingMessage message);
    TIncomingMessage DeserializeIncomingMessage(byte[] message);
}
