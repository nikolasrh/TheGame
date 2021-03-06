using System.Net;

using Microsoft.Extensions.Logging;

using TheGame.Network;
using TheGame.DedicatedServer;

var cancellationTokenSource = new CancellationTokenSource();
var cancellationToken = cancellationTokenSource.Token;

var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddConsole();
});

var serverMessageQueue = new ServerMessageQueue(loggerFactory.CreateLogger<ServerMessageQueue>());
var game = new Game(serverMessageQueue, loggerFactory.CreateLogger<Game>());
var gameEventHandler = new GameEventHandler(game, serverMessageQueue);
var serverCallbacks = new GameServerCallbacks(gameEventHandler);
var server = new Server(IPAddress.Any, 6000, serverCallbacks, loggerFactory.CreateLogger<Server>());

var serverLoop = new GameLoop(new GameLoopOptions(1), loggerFactory.CreateLogger<GameLoop>());

server.Start(serverLoop);

var gameLoop = new GameLoop(new GameLoopOptions(1), loggerFactory.CreateLogger<GameLoop>());

game.Start(gameLoop, server);
