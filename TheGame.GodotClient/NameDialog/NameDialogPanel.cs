using Godot;

namespace TheGame.GodotClient.NameDialog;

public partial class NameDialogPanel : Panel
{
    private LineEdit _inputField;

    [Signal]
    public delegate void NameSubmittedEventHandler(string name);

    public override void _Ready()
    {
        var inputField = GetNode<LineEdit>("MarginContainer/HBoxContainer/InputField");
        inputField.TextSubmitted += text => EmitSignal(SignalName.NameSubmitted, text);
    }

    public override void _Process(double delta)
    {
    }
}
