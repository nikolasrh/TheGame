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

        var inputField = GetNode<LineEdit>("MarginContainer/VBoxContainer/HBoxContainer/InputField");
        inputField.TextSubmitted += text => EmitSignal(SignalName.ChatMessageSubmitted, text);
    }

    public override void _Process(double delta)
    {
    }

    public void AddMessage(string text)
    {
        _richTextLabel.AddText(System.Environment.NewLine + text);
    }
}
