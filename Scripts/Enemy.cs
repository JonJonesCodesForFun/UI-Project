using System.Linq;
using Godot;

namespace UIProject.Scripts;

public partial class Enemy : Creature
{
	[Signal]
	public delegate void EnemyDiedEventHandler(int points);
	
	[Export]
	public int Points = 10;
	
	private bool IsAttacking => _sprite.Animation.ToString() == "attack";
	private bool HasTarget => _hurtBox.GetOverlappingBodies().Any(x => x is Player);
	
	private Player _player;
	private NavigationAgent2D _navAgent;
	private AnimatedSprite2D _sprite;
	private Area2D _hurtBox;
	private Timer _attackTimer;
	
	public override void _Ready()
	{
		CurrentHealth = MaxHealth;
		
		_player = GetTree().CurrentScene.GetNode<Player>("Player");
		if (_player == null)
			GD.PrintErr("Player not found");
		
		_navAgent = GetNode<NavigationAgent2D>("NavigationAgent2D");
		_sprite = GetNode<AnimatedSprite2D>("Sprite");
		_hurtBox = GetNode<Area2D>("HurtBox");
		_attackTimer = GetNode<Timer>("AttackTimer");
	}

	
	public override void _Process(double delta)
	{
		_navAgent.TargetPosition = _player.GlobalPosition;
		var nextPosition = _navAgent.GetNextPathPosition();
		
		Velocity = GlobalPosition.DirectionTo(nextPosition).Normalized() * Speed;

		if (!IsAttacking)
			UpdateDirection(Velocity);

		UpdateSpriteAnimation(Velocity, HasTarget);
		
		if (IsAttacking)
			Velocity = Vector2.Zero;
		
		MoveAndSlide();
	}

	public void TakeDamage(int damage)
	{
		GD.Print("enemy hit");
		CurrentHealth -= damage;
		if (CurrentHealth <= 0)
		{
			EmitSignal(SignalName.EnemyDied, Points);
			QueueFree();
		}
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
				//pause the animation to give a timer delay
				_sprite.Pause();
				_attackTimer.Start();
			}
		}
	}
	
	private void ResumeAttack()
	{
		_sprite.Play("attack");
	}
	
	private void DealDamage()
	{
		if (_sprite.Animation.ToString() == "attack")
		{
			var bodies = _hurtBox.GetOverlappingBodies();
			foreach (var body in bodies)
			{
				if (body is Player player)
				{
					//for this demo, just assume each attack does 1 damage
					player.TakeDamage(1);
				}
			}
			_sprite.Play("idle");
		}
	}
}