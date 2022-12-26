using Godot;


namespace TheGame.GodotClient.Characters;

public partial class Wizard : CharacterBody2D
{
    public const float Speed = 300.0f;
    public const float JumpVelocity = -400.0f;
    public static readonly float Gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();
    private static readonly Vector2 FlipX = new Vector2(-1, 1);
    [Export]
    private bool _playerControlled = false;
    [Export]
    private string _playerName;
    private AnimatedSprite2D _animatedSprite;
    private CollisionShape2D _collisionShape;
    private Label _label;

    [Signal]
    public delegate void PositionChangedEventHandler(Vector2 position, Vector2 velocity);

    public void SetPlayerName(string playerName)
    {
        _playerName = playerName;

        if (_label != null)
        {
            _label.Text = playerName;
        }
    }

    public override void _Ready()
    {
        _animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        _collisionShape = GetNode<CollisionShape2D>("CollisionShape2D");
        _label = GetNode<Label>("Label");
        _label.Text = _playerName;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!_playerControlled)
        {
            MoveAndSlide();
            UpdateAnimationWhenNotControlledByPlayer();
            return;
        }

        Vector2 velocity = Velocity;

        if (!IsOnFloor())
        {
            velocity.y += Gravity * (float)delta;

            _animatedSprite.Animation = velocity.y < 0 ? "jump" : "fall";
        }
        else
        {
            if (velocity.x == 0)
            {
                _animatedSprite.Animation = "idle";
            }
            else
            {
                _animatedSprite.Animation = "run";
            }
        }

        if (Input.IsActionJustPressed("player_jump") && IsOnFloor())
            velocity.y = JumpVelocity;

        float direction = Input.GetAxis("player_move_left", "player_move_right");
        if (direction != 0)
        {
            velocity.x = direction * Speed;
        }
        else
        {
            velocity.x = Mathf.MoveToward(Velocity.x, 0, Speed);
        }

        if (direction < 0)
        {
            if (_animatedSprite.FlipH == false)
            {
                _animatedSprite.FlipH = true;
                _collisionShape.Position = FlipX * _collisionShape.Position;
            }
        }
        if (direction > 0)
        {
            if (_animatedSprite.FlipH == true)
            {
                _animatedSprite.FlipH = false;
                _collisionShape.Position = FlipX * _collisionShape.Position;
            }
        }


        var oldPosition = Position;
        var oldVelocity = Velocity;

        Velocity = velocity;
        MoveAndSlide();

        if (oldPosition != Position || oldVelocity != Velocity)
        {
            EmitSignal(SignalName.PositionChanged, Position, Velocity);
        }
    }

    private void UpdateAnimationWhenNotControlledByPlayer()
    {
        if (!IsOnFloor())
        {
            _animatedSprite.Animation = Velocity.y < 0 ? "jump" : "fall";
        }
        else
        {
            if (Velocity.x == 0)
            {
                _animatedSprite.Animation = "idle";
            }
            else
            {
                _animatedSprite.Animation = "run";
            }
        }

        if (Velocity.x < 0)
        {
            if (_animatedSprite.FlipH == false)
            {
                _animatedSprite.FlipH = true;
                _collisionShape.Position = FlipX * _collisionShape.Position;
            }
        }
        if (Velocity.x > 0)
        {
            if (_animatedSprite.FlipH == true)
            {
                _animatedSprite.FlipH = false;
                _collisionShape.Position = FlipX * _collisionShape.Position;
            }
        }
    }
}
