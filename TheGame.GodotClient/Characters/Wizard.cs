using Godot;

public partial class Wizard : CharacterBody2D
{
    public const float Speed = 300.0f;
    public const float JumpVelocity = -400.0f;
    [Export]
    public bool PlayerName;
    private AnimatedSprite2D _animatedSprite;
    private CollisionShape2D _collisionShape;

    private Vector2 _flipX = new Vector2(-1, 1);

    public override void _Ready()
    {
        _animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        _collisionShape = GetNode<CollisionShape2D>("CollisionShape2D");
    }

    public override void _PhysicsProcess(double delta)
    {
        Vector2 velocity = Velocity;

        if (!IsOnFloor())
        {
            velocity.y += gravity * (float)delta;

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
                _collisionShape.Position = _flipX * _collisionShape.Position;
            }
        }
        if (direction > 0)
        {
            if (_animatedSprite.FlipH == true)
            {
                _animatedSprite.FlipH = false;
                _collisionShape.Position = _flipX * _collisionShape.Position;
            }
        }

        Velocity = velocity;
        MoveAndSlide();
    }
}
