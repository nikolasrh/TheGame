using System.Net.Sockets;

using Godot;

using Microsoft.Extensions.Logging;

using TheGame.Common;
using TheGame.GameClient;
using TheGame.NetworkConnection;
using TheGame.Protobuf;

public partial class GameNode : Node
{
    public Game Game;

    public override void _Ready()
    {
        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
        });

        var logger = loggerFactory.CreateLogger<GameNode>();

        var connection = new Connection<ServerMessage, ClientMessage>(
            new ClientMessageSerializer(),
            new TcpClient(),
            loggerFactory.CreateLogger<Connection<ServerMessage, ClientMessage>>());

        var loop = new Loop(new LoopOptions(10), loggerFactory.CreateLogger<Loop>());

        Game = new Game(loop, connection, loggerFactory.CreateLogger<Game>());
    }
}
