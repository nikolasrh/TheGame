using System.Net;
using System.Net.Sockets;

using Microsoft.Extensions.Logging;

using TheGame.Common;
using TheGame.GameClient;
using TheGame.NetworkConnection;
using TheGame.Protobuf;

var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddConsole();
});

var clientMessageSerializer = new ClientMessageSerializer();

var client = new TcpClient();
await client.ConnectAsync(new IPEndPoint(IPAddress.Loopback, 6000));

var clientConnectionCallbacks = new ClientConnectionCallbacks(loggerFactory.CreateLogger<ClientConnectionCallbacks>());

var connection = new Connection<ServerMessage, ClientMessage>(
    clientConnectionCallbacks,
    clientMessageSerializer,
    client,
    loggerFactory.CreateLogger<Connection<ServerMessage, ClientMessage>>());

var game = new Game(connection, loggerFactory.CreateLogger<Game>());

var loop = new Loop(new LoopOptions(10), loggerFactory.CreateLogger<Loop>());

game.Start(loop);
