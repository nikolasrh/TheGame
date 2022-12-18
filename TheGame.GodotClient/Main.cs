using System.Net;
using System.Net.Sockets;

using Godot;

using Microsoft.Extensions.Logging;

using TheGame.Common;
using TheGame.GodotClient.Chat;
using TheGame.GodotClient.NameDialog;
using TheGame.NetworkConnection;
using TheGame.Protobuf;

namespace TheGame.GodotClient;

public partial class Main : Control
{
    public override void _Ready()
    {
        var game = StartGame();

        var chat = GetNode<ChatPanel>("Chat");

        chat.ChatMessageSubmitted += (message) =>
        {
            const string EXIT_COMMAND = "/exit";
            const string NAME_COMMAND = "/name ";


            if (message == EXIT_COMMAND)
            {
                game.LeaveGame();
            }
            else if (message.StartsWith(NAME_COMMAND))
            {
                const int NAME_COMMAND_LENGTH = 6;
                var name = message.Substring(NAME_COMMAND_LENGTH);
                game.ChangeName(name);
            }
            else
            {
                game.SendChatMessage(message);
            }
        };

        var nameDialog = GetNode<NameDialogPanel>("NameDialog");
        nameDialog.NameSubmitted += game.JoinGame;

        game.PlayerJoinedEvent += player => chat.AddMessage($"{player.Name} joined");
        game.PlayerLeftEvent += player => chat.AddMessage($"{player.Name} left");
        game.PlayerUpdated += (oldPlayer, newPlayer) => chat.AddMessage($"{oldPlayer.Name} changed name to {newPlayer.Name}");
        game.ChatMesageReceived += (player, message) => chat.AddMessage($"{player.Name}: {message}");
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }

    public Game StartGame()
    {
        // TODO: Create logger using GD.Print
        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
        });

        var clientMessageSerializer = new ClientMessageSerializer();

        var client = new TcpClient();
        client.Connect(new IPEndPoint(IPAddress.Loopback, 6000));

        var connection = new Connection<ServerMessage, ClientMessage>(
            clientMessageSerializer,
            client,
            loggerFactory.CreateLogger<Connection<ServerMessage, ClientMessage>>());

        var loop = new Loop(new LoopOptions(10), loggerFactory.CreateLogger<Loop>());

        var game = new Game(loop, connection, loggerFactory.CreateLogger<Game>());
        game.Start();
        return game;
    }
}
