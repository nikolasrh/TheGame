using System.Net;

using Microsoft.Extensions.Logging;

using TheGame.Network;
using TheGame.DedicatedServer;

var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddConsole();
});

var serverCallbacks = new DedicatedServerCallbacks(loggerFactory.CreateLogger<DedicatedServerCallbacks>());

var server = new Server(IPAddress.Any, 6000, serverCallbacks, loggerFactory.CreateLogger<Server>());

await server.Start();
