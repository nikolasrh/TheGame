using Godot;

using System;

public partial class Wizard : CharacterBody2D
{
    public const float Speed = 300.0f;
    public const float JumpVelocity = -400.0f;

    // Get the gravity from the project settings to be synced with RigidBody nodes.
    public float gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();
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

        // Add the gravity.
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

        // Handle Jump.
        if (Input.IsActionJustPressed("player_jump") && IsOnFloor())
            velocity.y = JumpVelocity;

        // Get the input direction and handle the movement/deceleration.
        // As good practice, you should replace UI actions with custom gameplay actions.
        Vector2 direction = Input.GetVector("player_move_left", "player_move_right", null, null);
        if (direction != Vector2.Zero)
        {
            velocity.x = direction.x * Speed;
        }
        else
        {
            velocity.x = Mathf.MoveToward(Velocity.x, 0, Speed);
        }

        if (direction.x < 0)
        {
            if (_animatedSprite.FlipH == false)
            {
                _animatedSprite.FlipH = true;
                _collisionShape.Position = _flipX * _collisionShape.Position;
            }
        }
        if (direction.x > 0)
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
