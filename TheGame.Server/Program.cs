using System.Net;

using Microsoft.Extensions.Logging;

using TheGame.Server;

var cancellationTokenSource = new CancellationTokenSource();
var cancellationToken = cancellationTokenSource.Token;

var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddConsole();
});

var connectionManagerCallbacks = new ConnectionManagerCallbacks(loggerFactory.CreateLogger<ConnectionManagerCallbacks>());

var connectionManager = new ConnectionManager(IPAddress.Any, 6000, connectionManagerCallbacks, loggerFactory.CreateLogger<ConnectionManager>());

await connectionManager.Start(cancellationToken);
