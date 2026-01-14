using Godot;

namespace UIProject.Scripts;

public partial class Player : Creature
{
	[Signal]
	public delegate void LivesChangedEventHandler(int lives);


	[Export]
	public int Lives = 3;
	
	private Vector2 _startPosition;

	private bool IsAttacking => _sprite.Animation.ToString() == "attack" && _sprite.IsPlaying();

	private AnimatedSprite2D _sprite;
	private Area2D _hurtBox;

	public override void _Ready()
	{
		CurrentHealth = MaxHealth;
		_startPosition = GlobalPosition;
		
		_sprite = GetNode<AnimatedSprite2D>("Sprite");
		_hurtBox = GetNode<Area2D>("HurtBox");
	}

	public override void _PhysicsProcess(double delta)
	{
		var direction = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
		
		UpdateVelocity(direction);
		
		UpdateDirection(direction);
		
		var attacking = Input.IsActionJustPressed("ui_accept");
		//don't let the use spam attacks
		if (attacking && !IsAttacking)
			ActivateAttack();
		UpdateSpriteAnimation(direction, attacking);
		
		MoveAndSlide();
	}

	public void TakeDamage(int damage)
	{
		CurrentHealth -= damage;

		if (CurrentHealth <= 0)
		{
			Lives -= 1;
			EmitSignal(SignalName.LivesChanged, Lives);
			if (Lives <= 0) {
				GD.Print("Game Over");
				GetTree().Quit();
			}
			else
			{
				GD.Print($"Player Lives: {Lives}");
				GlobalPosition = _startPosition;
				CurrentHealth = MaxHealth;
			}
		}
		
		GD.Print($"Player Health: {CurrentHealth}");
		EmitSignal(Creature.SignalName.HealthChanged, CurrentHealth, MaxHealth);
	}
	
	private void UpdateVelocity(Vector2 direction)
	{
		Vector2 velocity = Velocity;
		if (direction != Vector2.Zero && !IsAttacking)
		{
			velocity.X = direction.X * Speed;
			velocity.Y = direction.Y * Speed;
			
		}
		else
		{
			velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
			velocity.Y = Mathf.MoveToward(Velocity.Y, 0, Speed);
		}
		Velocity = velocity;
	}
	
	private void UpdateDirection(Vector2 direction)
	{
		if (direction.X < 0)
		{
			_sprite.FlipH = true;
			if (_hurtBox.Position.X > 0)
				_hurtBox.Position = new Vector2(_hurtBox.Position.X * -1, _hurtBox.Position.Y);
		}
		else if (direction.X > 0)
		{
			_sprite.FlipH = false;
			if (_hurtBox.Position.X < 0)
				_hurtBox.Position = new Vector2(_hurtBox.Position.X * -1, _hurtBox.Position.Y);
		}
	}

	private void UpdateSpriteAnimation(Vector2 direction, bool attacking)
	{
		//don't interrupt the attack animation
		if (!IsAttacking)
		{
			if (direction != Vector2.Zero)
				_sprite.Play("walk");
			else
				_sprite.Play("idle");
			
			//attack needs to be checked first to get priority
			if (attacking)
			{
				_sprite.Play("attack");
				//stop moving if you're attacking
				Vector2 velocity = Vector2.Zero;
			}
		}
	}

	private void ActivateAttack()
	{
		var bodies = _hurtBox.GetOverlappingBodies();
		foreach (var body in bodies)
		{
			if (body is Enemy enemy)
			{
				//for this demo, just assume each attack does 1 damage
				enemy.TakeDamage(1);
			}
		}
	}
}