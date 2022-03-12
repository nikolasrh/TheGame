using System.Net;

using Microsoft.Extensions.Logging;

using TheGame.Network;
using TheGame.Server;

var cancellationTokenSource = new CancellationTokenSource();
var cancellationToken = cancellationTokenSource.Token;

var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddConsole();
});

var serverCallbacks = new ServerCallbacks(loggerFactory.CreateLogger<ServerCallbacks>());

var server = new Server(IPAddress.Any, 6000, serverCallbacks, loggerFactory.CreateLogger<Server>());

await server.Start(cancellationToken);
