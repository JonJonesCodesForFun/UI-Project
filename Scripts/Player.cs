using Godot;

namespace UIProject.Scripts;

public partial class Player : Creature
{
	[Signal]
	public delegate void LivesChangedEventHandler(int lives);

	[Export]
	public int Lives = 1;
	
	private Vector2 _startPosition;

	private bool IsAttacking => _sprite.Animation.ToString() == "attack" && _sprite.IsPlaying();

	private AnimatedSprite2D _sprite;
	private Area2D _hurtBox;

	private bool _isFlashing;

	private ProgressBar _progressBar;
	private Label _score;

	public override void _Ready()
	{
		CurrentHealth = MaxHealth;
		_startPosition = GlobalPosition;
		
		_sprite = GetNode<AnimatedSprite2D>("Sprite");
		_hurtBox = GetNode<Area2D>("HurtBox");

		_progressBar = GetNode<ProgressBar>("ProgressBar");
		_progressBar.MaxValue = MaxHealth;
		_progressBar.Value = CurrentHealth;
		
		_score = GetNode<Label>("score");
		_score.Text = "0";
	}

	public override void _PhysicsProcess(double delta)
	{
		var direction = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
		
		UpdateVelocity(direction);
		UpdateDirection(direction);
		
		var attacking = Input.IsActionJustPressed("ui_accept");
		if (attacking && !IsAttacking)
			ActivateAttack();
			
		UpdateSpriteAnimation(direction, attacking);
		MoveAndSlide();
	}

	public async void TakeDamage(int damage)
	{
		if (!_isFlashing)
		{
			_isFlashing = true;
			_sprite.Modulate = Colors.Red;
			await ToSignal(GetTree().CreateTimer(0.2f), "timeout");
			_sprite.Modulate = Colors.White;
			_isFlashing = false;
		}

		CurrentHealth -= damage;
		_progressBar.Value = CurrentHealth;

		if (CurrentHealth <= 0)
		{
			Lives -= 1;
			EmitSignal(SignalName.LivesChanged, Lives);

			
			if (Lives <= 0)
			{
				GetTree().ChangeSceneToFile("res://gameover.tscn");
				return; 
			}

			
			GlobalPosition = _startPosition;
			CurrentHealth = MaxHealth;
			_progressBar.Value = CurrentHealth;
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
		if (!IsAttacking)
		{
			if (direction != Vector2.Zero)
				_sprite.Play("walk");
			else
				_sprite.Play("idle");
			
			if (attacking)
				_sprite.Play("attack");
		}
	}

	private void ActivateAttack()
	{
		var bodies = _hurtBox.GetOverlappingBodies();
		foreach (var body in bodies)
		{
			if (body is Enemy enemy)
				enemy.TakeDamage(1);
		}
	}
	
	public void UpdateScore(int points) {
		var old_score = int.Parse(_score.Text);
		var new_score = old_score + points;
		
		_score.Text = new_score.ToString();
	}
}
