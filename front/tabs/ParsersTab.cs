using Godot;
using System;

using MtgCardParser;

public partial class ParsersTab : TabBar
{
	static readonly PackedScene PNodeBasePS = ResourceLoader.Load("res://pnodes/PNodeBase.tscn") as PackedScene;
	
	#region Nodes
	
	public GraphEdit GraphEditNode { get; private set; }
	public ItemList ParsersListNode { get; private set; }
	
	#endregion
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		#region Node fetching
		
		GraphEditNode = GetNode<GraphEdit>("%GraphEdit");
		ParsersListNode = GetNode<ItemList>("%ParsersList");
		
		#endregion
		
//		OnTemplatesListItemActivated(0);
	}
	
	private void OnGraphEditConnectionRequest(StringName from_node, int from_port, StringName to_node, int to_port)
	{
		GraphEditNode.ConnectNode(from_node, from_port, to_node, to_port);
	}
	
	private void OnGraphEditDisconnectionRequest(StringName from_node, int from_port, StringName to_node, int to_port)
	{
		GraphEditNode.DisconnectNode(from_node, from_port, to_node, to_port);
		// Replace with function body.
	}

	#region Button listeners
	
	private void OnAddButtonPressed()
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
		// TODO? don't know if this is the correct way, seems kinda slow
		
		var pNodeW = ParsersListNode.GetItemMetadata(index).As<Wrapper<PNode>>();
		var pNode = pNodeW.Value;
		GraphEditNode.ClearConnections();
		
		foreach (var child in GraphEditNode.GetChildren())
			child.QueueFree();

		AddPNodeBase(pNodeW, true);
		GraphEditNode.ArrangeNodes();
	}
	
	private PNodeBase AddPNodeBase(Wrapper<PNode> pNodeW, bool ignoreTemplate=false) {
		var node = PNodeBasePS.Instantiate() as PNodeBase;
		
		GraphEditNode.AddChild(node);
		node.Load(pNodeW, ignoreTemplate);
		
		if (pNodeW.Value.IsTemplate && !ignoreTemplate) return node;
		// iterate over the children
		var childI = 0;
		foreach (var child in pNodeW.Value.Children) {
			var cNode = AddPNodeBase(new Wrapper<PNode>(child));
			GraphEditNode.ConnectNode(node.Name, childI, cNode.Name, 0);
			++childI;
		}
		return node;
	}
	
	[Signal]
	public delegate void ParserAddedEventHandler(Wrapper<PNode> pNodeW);
	
	private void OnMainProjectLoaded(Wrapper<Project> projectW)
	{
		var parsers = projectW.Value.Parsers;
		foreach (var p in parsers)
			GD.Print(p.IsTemplate);
		foreach (var parser in parsers)
			EmitSignal(SignalName.ParserAdded, new Wrapper<PNode>(parser));
	}
	
	private void OnParserAdded(Wrapper<PNode> pNodeW)
	{
		var pnode = pNodeW.Value;
		var i = ParsersListNode.AddItem(pnode.Name);
		ParsersListNode.SetItemMetadata(i, pNodeW);
	}
}




