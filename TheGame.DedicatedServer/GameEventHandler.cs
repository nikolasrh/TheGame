using TheGame.Protobuf;

namespace TheGame.DedicatedServer;

public class GameEventHandler
{
    private readonly Game _game;
    private readonly ServerMessageQueue _serverMessageQueue;
    public GameEventHandler(Game game, ServerMessageQueue serverMessageQueue)
    {
        _game = game;
        _serverMessageQueue = serverMessageQueue;
    }

    public void HandleDisconnect(Guid connectionId)
    {
        var player = _game.RemovePlayer(connectionId);

        if (player == null) return;

        var serverMessage = new ServerMessage
        {
            PlayerLeft = new Protobuf.PlayerLeft
            {
                Player = new Protobuf.Player
                {
                    Id = player.Value.Id.ToString(),
                    Name = player.Value.Name
                }
            }
        };

        _serverMessageQueue.Write(serverMessage);
    }

    public void HandleClientMessage(byte[] data, Guid connectionId)
    {
        var clientMessage = Serializer.Deserialize(data);

        switch (clientMessage.MessageCase)
        {
            case ClientMessage.MessageOneofCase.JoinGame:
                HandleJoinGame(clientMessage.JoinGame, connectionId);
                break;
            case ClientMessage.MessageOneofCase.SendChat:
                HandleSendChat(clientMessage.SendChat, connectionId);
                break;
        }
    }

    private void HandleJoinGame(JoinGame joinGame, Guid connectionId)
    {
        var player = new Player
        {
            Id = connectionId,
            Name = joinGame.Name
        };
        _game.AddPlayer(player);

        var serverMessage = new ServerMessage
        {
            PlayerJoined = new Protobuf.PlayerJoined
            {
                Player = new Protobuf.Player
                {
                    Id = player.Id.ToString(),
                    Name = player.Name
                }
            }
        };

        _serverMessageQueue.Write(serverMessage);
    }

    private void HandleSendChat(SendChat sendChat, Guid connectionId)
    {
        var player = _game.GetPlayer(connectionId);

        if (player == null) return;

        var serverMessage = new ServerMessage
        {
            Chat = new Protobuf.Chat
            {
                Player = new Protobuf.Player
                {
                    Id = player.Value.Id.ToString(),
                    Name = player.Value.Name
                },
                Text = sendChat.Text
            }
        };

        _serverMessageQueue.Write(serverMessage);
    }
}
