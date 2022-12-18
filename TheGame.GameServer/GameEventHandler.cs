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

        if (player is null) return;

        var serverMessage = new ServerMessage
        {
            PlayerLeft = new Protobuf.PlayerLeft
            {
                PlayerId = player.Value.ConnectionId.ToString()
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
            case ClientMessage.MessageOneofCase.ChangeName:
                HandleChangeName(clientMessageEvent.connectionId, clientMessageEvent.message.ChangeName);
                break;
            case ClientMessage.MessageOneofCase.SendChat:
                HandleSendChat(clientMessageEvent.connectionId, clientMessageEvent.message.SendChat);
                break;
        }
    }

    private void HandleJoinGame(Guid connectionId, JoinGame joinGame)
    {
        var newConnection = _game.RemoveNewConnection(connectionId);

        if (newConnection is null) return;

        var player = new Player(connectionId, joinGame.Name);
        var protobufPlayer = PlayerToProtobufPlayer(player);

        var playerJoinedMessage = new ServerMessage
        {
            PlayerJoined = new Protobuf.PlayerJoined
            {
                Player = protobufPlayer
            }
        };

        var existingPlayers = _game.GetPlayers();

        foreach (var existingPlayer in existingPlayers)
        {
            _server.SendMessage(existingPlayer.ConnectionId, playerJoinedMessage);
        }

        _game.AddPlayer(player);

        var syncPlayers = new SyncPlayers();
        syncPlayers.Players.Add(protobufPlayer);
        syncPlayers.Players.AddRange(existingPlayers.Select(PlayerToProtobufPlayer));
        var syncPlayersMessage = new ServerMessage
        {
            SyncPlayers = syncPlayers
        };

        _server.SendMessage(connectionId, syncPlayersMessage);
    }

    private void HandleLeaveGame(Guid connectionId)
    {
        _server.Disconnect(connectionId);
    }

    private void HandleChangeName(Guid connectionId, ChangeName changeName)
    {
        var updatedPlayer = new Player(connectionId, changeName.Name);
        if (_game.UpdatePlayer(updatedPlayer))
        {
            var serverMessage = new ServerMessage
            {
                PlayerUpdated = new Protobuf.PlayerUpdated
                {
                    Player = new Protobuf.Player
                    {
                        Id = connectionId.ToString(),
                        Name = changeName.Name
                    }
                }
            };

            _server.SendMessage(serverMessage);
        }
    }

    private void HandleSendChat(Guid connectionId, SendChat sendChat)
    {
        var player = _game.GetPlayer(connectionId);

        if (player is null) return;

        var serverMessage = new ServerMessage
        {
            Chat = new Protobuf.Chat
            {
                PlayerId = player.Value.ConnectionId.ToString(),
                Text = sendChat.Text
            }
        };

        _server.SendMessage(serverMessage);
    }

    private Protobuf.Player PlayerToProtobufPlayer(Player player)
    {
        return new Protobuf.Player
        {
            Id = player.ConnectionId.ToString(),
            Name = player.Name
        };
    }
}
