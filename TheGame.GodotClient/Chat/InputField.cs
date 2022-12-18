using Godot;

namespace TheGame.GodotClient.Chat;

public partial class InputField : LineEdit
{
    public override void _Ready()
    {
        TextSubmitted += _ => Clear();

        var submitButton = GetNode<Button>("../SubmitButton");
        submitButton.Pressed += () => EmitSignal(SignalName.TextSubmitted, Text);
    }

    public override void _Process(double delta)
    {
    }
}
