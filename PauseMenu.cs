using Godot;

public partial class PauseMenu : Control
{
	public override void _Process(double delta)
	{
		TestEsc();
	}

	public void Resume()
	{
		GetTree().Paused = false;
	}

	public void Pause()
	{
		GetTree().Paused = true;
	}

	private void TestEsc()
	{
		if (Input.IsActionJustPressed("escape"))
		{
			GD.Print("User pressed escape");
			if (GetTree().Paused)
				Resume();
			else
				Pause();
		}
	}
}
