using System.Net;
using System.Net.Sockets;

using Microsoft.Extensions.Logging;

using TheGame.Client;
using TheGame.Core;

var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddConsole();
});

var client = new TcpClient();
await client.ConnectAsync(new IPEndPoint(IPAddress.Loopback, 6000));
var connection = new Connection(client, loggerFactory.CreateLogger<Connection>());

var chat = new Chat(connection);

while (true)
{
    Console.Write("Message: ");
    var message = Console.ReadLine() ?? string.Empty;
    await chat.Send(message);
}

// var gameLoopOptions = new GameLoopOptions(tickRate: 1);
// var gameLoop = new GameLoop(gameLoopOptions, loggerFactory.CreateLogger<GameLoop>());
// var gameLoopTask = gameLoop.Run((TimeSpan delta) => { });
// await gameLoopTask;
