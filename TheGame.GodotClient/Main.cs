using Godot;

using TheGame.GodotClient.Chat;

namespace TheGame.GodotClient;

public partial class Main : Control
{
    private static readonly string ExitCommand = "/exit";
    private static readonly string NameCommand = "/name";
    private static readonly int NameCommandLength = NameCommand.Length;

    public override void _Ready()
    {
        var game = GetNode<GameNode>("/root/GameNode").Game;
        var chat = GetNode<ChatPanel>("Chat");

        chat.ChatMessageSubmitted += (message) =>
        {
            if (message == ExitCommand)
            {
                game.Disconnect();
            }
            else if (message.StartsWith(NameCommand))
            {
                var name = message.Substring(NameCommandLength);
                game.ChangeName(name);
            }
            else
            {
                game.SendChatMessage(message);
            }
        };

        game.PlayerJoined += player => chat.AddMessage($"{player.Name} joined");
        game.PlayerLeft += player => chat.AddMessage($"{player.Name} left");
        game.PlayerUpdated += (oldPlayer, newPlayer) =>
        {
            if (oldPlayer.Name != newPlayer.Name)
            {
                chat.AddMessage($"{oldPlayer.Name} changed name to {newPlayer.Name}");
            }
        };
        game.ChatMesageReceived += (player, message) => chat.AddMessage($"{player.Name}: {message}");
    }
}
