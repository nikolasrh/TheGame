using Godot;

public partial class TextInput : MarginContainer
{
    [Export]
    private string _label;
    [Export]
    public string Value { get; private set; }

    public override void _Ready()
    {
        var label = GetNode<Label>("VBoxContainer/Label");
        var lineEdit = GetNode<LineEdit>("VBoxContainer/LineEdit");

        label.Text = _label;
        lineEdit.Text = Value;

        lineEdit.TextChanged += newText => Value = newText;
    }
}
