using Godot;
using System;

public partial class TTArgListItem : Control
{
	#region Nodes
	
	public Label ArgNameLabelNode { get; private set; }
	public LineEdit ArgValueEditNode { get; private set; }
	
	#endregion
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		#region Node fetching
		
		ArgNameLabelNode = GetNode<Label>("%ArgNameLabel");
		ArgValueEditNode = GetNode<LineEdit>("%ArgValueEdit");
		
		#endregion
	}

}
