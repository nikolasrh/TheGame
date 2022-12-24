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

        var playerLeftMessage = new ServerMessage
        {
            PlayerLeft = new Protobuf.PlayerLeft
            {
                PlayerId = player.Value.ConnectionId.ToString()
            }
        };

        _server.QueueMessage(playerLeftMessage);
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
            case ClientMessage.MessageOneofCase.Move:
                HandleMove(clientMessageEvent.connectionId, clientMessageEvent.message.Move);
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
            _server.QueueMessage(existingPlayer.ConnectionId, playerJoinedMessage);
        }

        _game.AddPlayer(player);

        var gameState = new GameState();
        gameState.Players.Add(protobufPlayer);
        gameState.Players.AddRange(existingPlayers.Select(PlayerToProtobufPlayer));

        var welcomeMessage = new ServerMessage
        {
            Welcome = new Welcome
            {
                PlayerId = player.ConnectionId.ToString(),
                GameState = gameState
            }
        };

        _server.QueueMessage(connectionId, welcomeMessage);
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
            var playerUpdatedMessage = new ServerMessage
            {
                PlayerUpdated = new PlayerUpdated
                {
                    Player = new Protobuf.Player
                    {
                        Id = connectionId.ToString(),
                        Name = changeName.Name
                    }
                }
            };

            _server.QueueMessage(playerUpdatedMessage);
        }
    }

    private void HandleSendChat(Guid connectionId, SendChat sendChat)
    {
        var player = _game.GetPlayer(connectionId);

        if (player is null) return;

        var chatMessage = new ServerMessage
        {
            Chat = new Protobuf.Chat
            {
                PlayerId = player.Value.ConnectionId.ToString(),
                Text = sendChat.Text
            }
        };

        _server.QueueMessage(chatMessage);
    }

    private void HandleMove(Guid connectionId, Move move)
    {
        var player = _game.GetPlayer(connectionId);

        if (player is null) return;

        var playedMovedMessage = new ServerMessage
        {
            PlayerMoved = new Protobuf.PlayerMoved
            {
                PlayerId = player.Value.ConnectionId.ToString(),
                X = move.X,
                Y = move.Y
            }
        };

        _server.QueueMessage(playedMovedMessage);
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
