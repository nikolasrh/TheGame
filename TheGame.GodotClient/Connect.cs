using Godot;

public partial class Connect : VBoxContainer
{
    public override void _Ready()
    {
        var game = GetNode<GameNode>("/root/GameNode").Game;
        var hostname = GetNode<TextInput>("Hostname");
        var port = GetNode<NumberInput>("Port");
        var playerName = GetNode<TextInput>("PlayerName");
        var submitButton = GetNode<Button>("MarginContainer/Button");

        submitButton.Pressed += () =>
        {
            game.Connect(hostname.Value, port.Value);
            game.JoinGame(playerName.Value);
            GetTree().ChangeSceneToFile("res://Main.tscn");
        };
    }
}
