using Godot;
using System;

namespace FrontV2;

public partial class Root : Control
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		
	}
	
	public override void _Input(InputEvent e) {
		if (e.IsActionPressed("quit")) {
			GetTree().Quit();
		}
	}
}
