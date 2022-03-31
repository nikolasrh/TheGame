using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using TheGame.Core;
using TheGame.Network;
using TheGame.Protobuf;

namespace TheGame.MonoGameClient
{
    public static class Program
    {
        [STAThread]
        static async Task Main()
        {
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
            });

            var client = new TcpClient();
            await client.ConnectAsync(new IPEndPoint(IPAddress.Loopback, 6000));

            var player = new Core.Player(Guid.NewGuid(), "Test");
            var playerGameState = new PlayerGameState();
            // playerGameState.AddPlayer(player);

            var connectionCallbacks = new ConnectionCallbacks(playerGameState, loggerFactory.CreateLogger<ConnectionCallbacks>());

            var connection = new Connection(client, connectionCallbacks, loggerFactory.CreateLogger<Connection>());

            await SendJoinGame(player, connection);

            var connectionStartTask = connection.Start();

            var sendClientPositionTask = Task.Run(async () =>
            {
                await Task.Delay(200);
                await SendPosition(player, connection);
            });

            using (var game = new Game1(player.Id, playerGameState, connection))
                game.Run();
        }

        private static async Task SendJoinGame(Core.Player player, Connection connection)
        {
            var message = new ClientMessage
            {
                JoinGame = new()
                {
                    Player = new()
                    {
                        Id = player.Id.ToString(),
                        Name = player.Name
                    }
                }
            };
            await connection.Write(Serializer.Serialize(message));
        }

        private static async Task SendPosition(Core.Player player, Connection connection)
        {
            var message = new ClientMessage
            {
                Position = new Position
                {
                    X = player.PositionX,
                    Y = player.PositionY
                }
            };
            await connection.Write(Serializer.Serialize(message));
        }
    }
}
