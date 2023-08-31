using Godot;
using System;

public partial class ParsersTab : TabBar
{
	#region Nodes
	
	public GraphEdit GraphEditNode { get; private set; }
	
	#endregion
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		#region Node fetching
		
		GraphEditNode = GetNode<GraphEdit>("%GraphEdit");
		
		#endregion
	}
	
	private void OnGraphEditConnectionRequest(StringName from_node, int from_port, StringName to_node, int to_port)
	{
		GraphEditNode.ConnectNode(from_node, from_port, to_node, to_port);
	}

	#region Button listeners
	
	private void OnAddButtonPressed()
	{
		// TODO
	}

	private void OnEditButtonPressed()
	{
		// TODO
	}

	private void OnRemoveButtonPressed()
	{
		// TODO
	}
	
	#endregion
	
	private void OnTemplatesListItemActivated(int index)
	{
		// TODO
	}
}







