using System.Net;

using Microsoft.Extensions.Logging;

using TheGame.Core;
using TheGame.Server;

var cancellationTokenSource = new CancellationTokenSource();
var cancellationToken = cancellationTokenSource.Token;

var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddConsole();
});

var connectionManager = new ConnectionManager(IPAddress.Any, 6000, loggerFactory.CreateLogger<ConnectionManager>());

var connectionManagerRunTask = connectionManager.Run(cancellationToken);

var gameLoopOptions = new GameLoopOptions(tickRate: 1);
var logger = loggerFactory.CreateLogger<GameLoop>();

var gameLoop = new GameLoop(gameLoopOptions, logger);

var gameLoopTask = gameLoop.Run((TimeSpan delta) => { });

Task.WaitAny(connectionManagerRunTask, gameLoopTask);
