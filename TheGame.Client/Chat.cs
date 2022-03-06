using TheGame.Core;
using TheGame.Core.Serialization;

namespace TheGame.Client;

public class Chat
{
    private readonly Connection _connection;

    public Chat(Connection connection)
    {
        _connection = connection;
    }

    public async Task Send(string message)
    {
        var gameEvent = new GameEvent
        {
            ChatMessage = new ChatMessage
            {
                Text = message
            }
        };
        var serializedGameEvent = Serializer.Serialize(gameEvent);

        await _connection.Write(serializedGameEvent);
    }
}
