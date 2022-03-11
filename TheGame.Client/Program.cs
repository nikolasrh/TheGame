using System.Net;
using System.Net.Sockets;

using Microsoft.Extensions.Logging;

using TheGame.Client;
using TheGame.Core;
using TheGame.Protobuf;

var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddConsole();
});
var logger = loggerFactory.CreateLogger<Program>();

var client = new TcpClient();
await client.ConnectAsync(new IPEndPoint(IPAddress.Loopback, 6000));
var connection = new Connection(client, loggerFactory.CreateLogger<Connection>());

var onRead = (Connection connection, byte[] data) =>
{
    var serverMessage = Serializer.Deserialize(data);

    switch (serverMessage.MessageCase)
    {
        case ServerMessage.MessageOneofCase.Chat:
            var chat = serverMessage.Chat;
            logger.LogInformation("{playerId}: {text}", chat.Player.Id, chat.Text);
            break;
        case ServerMessage.MessageOneofCase.PlayerJoined:
            var playerJoined = serverMessage.PlayerJoined;
            logger.LogInformation("Player {playerId} joined", playerJoined.Player.Id);
            break;
        case ServerMessage.MessageOneofCase.PlayerLeft:
            var playerLeft = serverMessage.PlayerLeft;
            logger.LogInformation("Player {playerId} left", playerLeft.Player.Id);
            break;
    }

    return Task.CompletedTask;
};

var _ = Task.WhenAll(connection.StartWriting(), connection.StartReading(onRead));

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
