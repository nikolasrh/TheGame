using System.Net;

using Microsoft.Extensions.Logging;

using TheGame.Core;
using TheGame.DedicatedServer;
using TheGame.Network;

var cancellationTokenSource = new CancellationTokenSource();
var cancellationToken = cancellationTokenSource.Token;

var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddConsole();
});

var playerGameState = new PlayerGameState();

var serverCallbacks = new DedicatedServerCallbacks(playerGameState, loggerFactory.CreateLogger<DedicatedServerCallbacks>());

var server = new Server(IPAddress.Any, 6000, serverCallbacks, loggerFactory.CreateLogger<Server>());

await server.Start(cancellationToken);
