using System.Net;
using System.Net.Sockets;

using Microsoft.Extensions.Logging;

using TheGame.Client;
using TheGame.Network;

var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddConsole();
});

var client = new TcpClient();
await client.ConnectAsync(new IPEndPoint(IPAddress.Loopback, 6000));

var connectionCallbacks = new ConnectionCallbacks(loggerFactory.CreateLogger<ConnectionCallbacks>());
var connection = new Connection(client, connectionCallbacks, loggerFactory.CreateLogger<Connection>());

var game = new Game(connection, loggerFactory.CreateLogger<Game>());
game.Run();

// var gameLoopOptions = new GameLoopOptions(tickRate: 1);
// var gameLoop = new GameLoop(gameLoopOptions, loggerFactory.CreateLogger<GameLoop>());
// var gameLoopTask = gameLoop.Run((TimeSpan delta) => { });
// await gameLoopTask;
