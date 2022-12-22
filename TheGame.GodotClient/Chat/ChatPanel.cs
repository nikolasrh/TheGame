using Godot;

namespace TheGame.GodotClient.Chat;

public partial class ChatPanel : Panel
{
    private RichTextLabel _richTextLabel;

    [Signal]
    public delegate void ChatMessageSubmittedEventHandler(string text);

    public override void _Ready()
    {
        _richTextLabel = GetNode<RichTextLabel>("MarginContainer/VBoxContainer/RichTextLabel");

        var chatBar = GetNode<ChatBar>("MarginContainer/VBoxContainer/HBoxContainer/LineEdit");
        chatBar.TextSubmitted += text => EmitSignal(SignalName.ChatMessageSubmitted, text);
    }

    public void AddMessage(string text)
    {
        _richTextLabel.AddText(System.Environment.NewLine + text);
    }
}
