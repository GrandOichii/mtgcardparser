using Godot;
using System;


public partial class PNodeBase : GraphNode
{
	public override void _Ready()
	{
		SetSlot(0, true, 0, new Color(1, 1, 1, 1), true, 0, new Color(0, 1, 0, 1));
	}

}
