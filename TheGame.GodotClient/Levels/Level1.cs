using System.Collections.Generic;

using Godot;

using TheGame.GameClient;
using TheGame.GodotClient.Characters;
using TheGame.Protobuf;

namespace TheGame.GodotClient.Levels;

public partial class Level1 : Node
{
    private Game _game;
    private Wizard _player;
    private Node _otherPlayersNode;
    private PackedScene _wizardScene;
    private readonly Dictionary<string, Wizard> _otherPlayers = new();

    public override void _Ready()
    {
        _wizardScene = (PackedScene)ResourceLoader.Load("res://Characters/Wizard.tscn");
        _game = GetNode<GameNode>("/root/GameNode").Game;
        _otherPlayersNode = GetNode<Node>("Map/Players");

        _player = GetNode<Wizard>("Map/Player");
        _player.PositionChanged += pos => _game.Move(pos.x, pos.y);

        _game.PlayerMoved += UpdatePlayerPosition;
        _game.PlayerUpdated += UpdatePlayerName;
        _game.PlayerJoined += CreatePlayer;
        _game.PlayerLeft += RemovePlayer;
        _game.GameJoined += CreatePlayers;
    }

    private void CreatePlayers(string playerId, IEnumerable<Player> players)
    {
        foreach (var player in players)
        {
            if (playerId == player.Id)
            {
                _player.SetPlayerName(player.Name);
            }
            else
            {
                CreatePlayer(player);
            }
        }
    }

    private void CreatePlayer(Player player)
    {

        var wizard = _wizardScene.Instantiate<Wizard>();
        wizard.SetPlayerName(player.Name);
        _otherPlayers.Add(player.Id, wizard);
        _otherPlayersNode.AddChild(wizard);
    }

    private void UpdatePlayerName(Player oldPlayer, Player newPlayer)
    {
        if (oldPlayer.Name != newPlayer.Name)
        {
            if (_otherPlayers.TryGetValue(oldPlayer.Id, out var wizard))
            {
                wizard.SetPlayerName(newPlayer.Name);
            }
        }
    }

    private void UpdatePlayerPosition(Player player, float x, float y)
    {
        if (_otherPlayers.TryGetValue(player.Id, out var wizard))
        {
            wizard.Position = new Vector2(x, y);
        }
    }

    private void RemovePlayer(Player player)
    {
        if (_otherPlayers.Remove(player.Id, out var wizard))
        {
            wizard.QueueFree();
        }
    }
}
