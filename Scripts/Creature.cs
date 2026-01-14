using Godot;

namespace UIProject.Scripts;

public partial class Creature : CharacterBody2D
{
	[Signal]
	public delegate void HealthChangedEventHandler(int currentHealth, int maxHealth);

	[Export]
	public int MaxHealth = 3;
	[Export]
	public float Speed = 300.0f;
	
	protected int CurrentHealth;
}