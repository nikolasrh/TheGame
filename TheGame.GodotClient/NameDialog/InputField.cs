using Godot;

namespace TheGame.GodotClient.NameDialog;

public partial class InputField : LineEdit
{
    public override void _Ready()
    {
        var submitButton = GetNode<Button>("../SubmitButton");
        submitButton.Pressed += () => EmitSignal(SignalName.TextSubmitted, Text);

        TextSubmitted += _ =>
        {
            Editable = false;
            submitButton.Disabled = true;
        };
    }

    public override void _Process(double delta)
    {
    }
}
