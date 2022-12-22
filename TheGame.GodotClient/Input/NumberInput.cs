using Godot;

public partial class NumberInput : MarginContainer
{
    [Export]
    private string _label;
    [Export]
    public int Value { get; private set; }

    public override void _Ready()
    {
        var label = GetNode<Label>("VBoxContainer/Label");
        var lineEdit = GetNode<LineEdit>("VBoxContainer/LineEdit");

        label.Text = _label;
        lineEdit.Text = Value.ToString();

        lineEdit.TextChanged += newText =>
        {
            if (int.TryParse(newText, out var newValue))
            {
                Value = newValue;
            }
            else
            {
                var text = Value.ToString();
                lineEdit.Text = text;
                lineEdit.CaretColumn = text.Length;
            }
        };
    }
}
