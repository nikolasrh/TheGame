using Godot;

using TheGame.GameClient;
using TheGame.Protobuf;

namespace TheGame.GodotClient.Chat;

public partial class ChatPanel : Panel
{
    private static readonly string ExitCommand = "/exit";
    private static readonly string QuitCommand = "/quit";
    private static readonly string NameCommand = "/name";
    private static readonly int NameCommandLength = NameCommand.Length;
    private Game _game;
    private RichTextLabel _richTextLabel;

    public override void _Ready()
    {
        _game = GetNode<GameNode>("/root/GameNode").Game;
        _game.PlayerJoined += PlayedJoined;
        _game.PlayerLeft += PlayerLeft;
        _game.PlayerUpdated += PlayerUpdated;
        _game.ChatMesageReceived += ChatMessageReceived;

        _richTextLabel = GetNode<RichTextLabel>("MarginContainer/VBoxContainer/RichTextLabel");

        var chatBar = GetNode<ChatBar>("MarginContainer/VBoxContainer/HBoxContainer/LineEdit");
        chatBar.TextSubmitted += ChatMessageSubmitted;
    }

    private void ChatMessageSubmitted(string message)
    {
        if (message == ExitCommand || message == QuitCommand)
        {
            _game.Disconnect();
        }
        else if (message.StartsWith(NameCommand))
        {
            var name = message.Substring(NameCommandLength);
            _game.ChangeName(name);
        }
        else
        {
            _game.SendChatMessage(message);
        }
    }

    private void AddMessage(string text)
    {
        _richTextLabel.AddText(System.Environment.NewLine + text);
    }

    private void PlayedJoined(Player player)
    {
        AddMessage($"{player.Name} joined");
    }

    private void PlayerLeft(Player player)
    {
        AddMessage($"{player.Name} left");
    }

    private void PlayerUpdated(Player oldPlayer, Player newPlayer)
    {
        if (oldPlayer.Name != newPlayer.Name)
        {
            AddMessage($"{oldPlayer.Name} changed name to {newPlayer.Name}");
        }
    }

    private void ChatMessageReceived(Player player, string message)
    {
        AddMessage($"{player.Name}: {message}");
    }

    public override void _ExitTree()
    {
        _game.PlayerJoined -= PlayedJoined;
        _game.PlayerLeft -= PlayerLeft;
        _game.PlayerUpdated -= PlayerUpdated;
        _game.ChatMesageReceived -= ChatMessageReceived;

        base._ExitTree();
    }
}
