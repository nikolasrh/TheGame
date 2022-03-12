using System.Net;
using System.Net.Sockets;

using Microsoft.Extensions.Logging;

using TheGame.Client;
using TheGame.Network;
using TheGame.Protobuf;

using ConnectionCallbacks = TheGame.Client.ConnectionCallbacks;

var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddConsole();
});
var logger = loggerFactory.CreateLogger<Program>();

var client = new TcpClient();
await client.ConnectAsync(new IPEndPoint(IPAddress.Loopback, 6000));

var connectionCallbacks = new ConnectionCallbacks(loggerFactory.CreateLogger<ConnectionCallbacks>());

var connection = new Connection(client, connectionCallbacks, loggerFactory.CreateLogger<Connection>());

var _ = connection.Start();

while (true)
{
    var message = Console.ReadLine() ?? string.Empty;

    var serializedClientMessage = Serializer.Serialize(new ClientMessage
    {
        SendChat = new SendChat
        {
            Text = message
        }
    });

    await connection.Write(serializedClientMessage);
}

// var gameLoopOptions = new GameLoopOptions(tickRate: 1);
// var gameLoop = new GameLoop(gameLoopOptions, loggerFactory.CreateLogger<GameLoop>());
// var gameLoopTask = gameLoop.Run((TimeSpan delta) => { });
// await gameLoopTask;
