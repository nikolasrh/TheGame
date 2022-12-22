using Godot;

namespace TheGame.GodotClient.Chat;

public partial class ChatBar : LineEdit
{
    public override void _Ready()
    {
        TextSubmitted += _ => Clear();

        var submitButton = GetNode<Button>("../Button");
        submitButton.Pressed += () => EmitSignal(SignalName.TextSubmitted, Text);
    }
}
