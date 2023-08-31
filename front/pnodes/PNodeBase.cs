using Godot;
using System;

public partial class PNodeBase : GraphNode
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		SetSlot(
			0, 
			true, 0, new Color(1, 0, 0),
			true, 0, new Color(0, 1, 0)
		);
		GD.Print("mogus");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
