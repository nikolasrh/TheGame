using Godot;

public partial class Connect : VBoxContainer
{
    public override void _Ready()
    {
        var game = GetNode<GameNode>("/root/GameNode").Game;
        var hostname = GetNode<TextInput>("VBoxContainer/HostnameInput");
        var port = GetNode<NumberInput>("VBoxContainer/PortInput");
        var playerName = GetNode<TextInput>("VBoxContainer/PlayerNameInput");
        var connectButton = GetNode<Button>("ConnectButton");
        var errorMessage = GetNode<Label>("ErrorMessage");
        errorMessage.Visible = false;

        connectButton.Pressed += () =>
        {
            if (game.Connect(hostname.Value, port.Value))
            {
                game.JoinGame(playerName.Value);
                GetTree().ChangeSceneToFile("res://Levels/Level1.tscn");
            }
            else
            {
                errorMessage.Visible = true;
            }
        };
    }
}
