using System.Net;

using Microsoft.Extensions.Logging;

using TheGame.NetworkServer;
using TheGame.GameServer;
using TheGame.Protobuf;
using System.Collections.Concurrent;
using TheGame.Common;

var cancellationTokenSource = new CancellationTokenSource();
var cancellationToken = cancellationTokenSource.Token;

var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddConsole();
});

var clientMessageEventQueue = new ConcurrentQueue<ClientMessageEvent>();
var connectionEventQueue = new ConcurrentQueue<ConnectionEvent>();
var serverMessageSerializer = new ServerMessageSerializer();

var server = new Server<ClientMessage, ServerMessage>(
    IPAddress.Any,
    6000,
    serverMessageSerializer,
    loggerFactory.CreateLogger<Server<ClientMessage, ServerMessage>>());

var loop = new Loop(new LoopOptions(10), loggerFactory.CreateLogger<Loop>());

var game = new Game(
    clientMessageEventQueue,
    connectionEventQueue,
    server,
    loggerFactory.CreateLogger<Game>());

var gameEventHandler = new GameEventHandler(game, server);

server.Start(loop);
game.Start(loop);
