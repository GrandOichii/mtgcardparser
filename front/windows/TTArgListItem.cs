using Godot;
using System;

public partial class TTArgListItem : Control
{
	#region Nodes
	
	public Label ArgNameLabelNode { get; private set; }
	public LineEdit ArgValueEditNode { get; private set; }
	
	#endregion
	
	public override void _Ready()
	{
		#region Node fetching
		
		ArgNameLabelNode = GetNode<Label>("%ArgNameLabel");
		ArgValueEditNode = GetNode<LineEdit>("%ArgValueEdit");
		
		#endregion
	}
	
	public void Load(string argName, string argValue) {
		ArgNameLabelNode.Text = argName;
		ArgValueEditNode.Text = argValue;
	}

}
