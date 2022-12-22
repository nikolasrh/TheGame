using Godot;

using TheGame.GodotClient.Chat;
using TheGame.GodotClient.NameDialog;


namespace TheGame.GodotClient;

public partial class Main : Control
{
    public override void _Ready()
    {
        var game = GetNode<GameNode>("/root/GameNode").Game;
        var chat = GetNode<ChatPanel>("Chat");

        chat.ChatMessageSubmitted += (message) =>
        {
            const string EXIT_COMMAND = "/exit";
            const string NAME_COMMAND = "/name ";

            if (message == EXIT_COMMAND)
            {
                game.Disconnect();
            }
            else if (message.StartsWith(NAME_COMMAND))
            {
                const int NAME_COMMAND_LENGTH = 6;
                var name = message.Substring(NAME_COMMAND_LENGTH);
                game.ChangeName(name);
            }
            else
            {
                game.SendChatMessage(message);
            }
        };

        var nameDialog = GetNode<NameDialogPanel>("NameDialog");
        nameDialog.NameSubmitted += name =>
        {
            if (game.Connect("localhost", 6000))
            {
                game.JoinGame(name);
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

    public override void _Process(double delta)
    {
    }
}
