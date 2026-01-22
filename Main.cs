using Godot;

public partial class Main : Control
{
	public override void _Ready()
	{
	}

	private void _on_start_pressed()
	{
		GetTree().ChangeSceneToFile("res://scenes/level.tscn");
	}
}
