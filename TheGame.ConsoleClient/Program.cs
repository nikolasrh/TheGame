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
var logger = loggerFactory.CreateLogger<Program>();

var connection = new Connection<ServerMessage, ClientMessage>(
    new ClientMessageSerializer(),
    new TcpClient(),
    loggerFactory.CreateLogger<Connection<ServerMessage, ClientMessage>>());

var loop = new Loop(new LoopOptions(10), loggerFactory.CreateLogger<Loop>());

var game = new Game(loop, connection, loggerFactory.CreateLogger<Game>());

game.ChatMesageReceived += (player, message) =>
{
    logger.LogInformation("{player}: {text}", player.Name, message);
};
game.PlayerJoined += (player) =>
{
    logger.LogInformation("{player} connected", player.Name);
};
game.PlayerLeft += (player) =>
{
    logger.LogInformation("{player} disconnected", player.Name);
};
game.PlayerUpdated += (oldPlayer, newPlayer) =>
{
    logger.LogInformation("{oldPlayer} changed name to {player}", oldPlayer.Name, newPlayer.Name);
};

Console.Write("Name: ");
var playerName = Console.ReadLine() ?? "Console";

if (!game.Connect("localhost", 6000)) return;

logger.LogInformation("Connected!");

game.JoinGame(playerName);

const string EXIT_COMMAND = "/exit";
const string NAME_COMMAND = "/name ";
const int NAME_COMMAND_LENGTH = 6;

while (game.Connected)
{
    var message = Console.ReadLine() ?? string.Empty;

    if (message == EXIT_COMMAND)
    {
        game.Disconnect();
        break;
    }
    else if (message.StartsWith(NAME_COMMAND))
    {
        var name = message.Substring(NAME_COMMAND_LENGTH);
        game.ChangeName(name);
    }
    else
    {
        game.SendChatMessage(message);
    }
}
