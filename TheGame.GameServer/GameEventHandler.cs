using TheGame.NetworkServer;
using TheGame.Protobuf;

namespace TheGame.GameServer;

public class GameEventHandler
{
    private readonly Game _game;
    private readonly Server<ClientMessage, ServerMessage> _server;

    public GameEventHandler(Game game, Server<ClientMessage, ServerMessage> server)
    {
        _game = game;
        _server = server;
    }

    public void HandleConnectionEvent(ConnectionEvent connectionEvent)
    {
        switch (connectionEvent.type)
        {
            case ConnectionEventType.CONNECT:
                HandleClientConnection(connectionEvent.connectionId);
                break;
            case ConnectionEventType.DISCONNECT:
                HandleClientDisconnect(connectionEvent.connectionId);
                break;
        }
    }

    private void HandleClientConnection(Guid connectionId)
    {
        _game.AddNewConnection(connectionId);
    }

    private void HandleClientDisconnect(Guid connectionId)
    {
        _game.RemoveNewConnection(connectionId);
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

        _server.SendMessage(serverMessage);
    }

    public void HandleClientMessageEvent(ClientMessageEvent clientMessageEvent)
    {
        switch (clientMessageEvent.message.MessageCase)
        {
            case ClientMessage.MessageOneofCase.JoinGame:
                HandleJoinGame(clientMessageEvent.connectionId, clientMessageEvent.message.JoinGame);
                break;
            case ClientMessage.MessageOneofCase.LeaveGame:
                HandleLeaveGame(clientMessageEvent.connectionId);
                break;
            case ClientMessage.MessageOneofCase.SendChat:
                HandleSendChat(clientMessageEvent.connectionId, clientMessageEvent.message.SendChat);
                break;
        }
    }

    private void HandleJoinGame(Guid connectionId, JoinGame joinGame)
    {
        var newConnection = _game.RemoveNewConnection(connectionId);

        if (newConnection == null) return;

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

        _server.SendMessage(serverMessage);
    }

    private void HandleLeaveGame(Guid connectionId)
    {
        _server.Disconnect(connectionId);
    }

    private void HandleSendChat(Guid connectionId, SendChat sendChat)
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

        _server.SendMessage(serverMessage);
    }
}