using Microsoft.Extensions.Logging;

using TheGame.Core;

var gameLoopOptions = new GameLoopOptions(tickRate: 1);

var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddConsole();
});

var logger = loggerFactory.CreateLogger<GameLoop>();

var gameLoop = new GameLoop(gameLoopOptions, logger);
await gameLoop.Run((TimeSpan delta) => { });
