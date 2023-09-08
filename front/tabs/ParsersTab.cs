using Godot;
using System;
using System.Collections.Generic;

using MtgCardParser;


// TODO
// reselecting another parser keeps moving the whole tree SE for some reason


public partial class ParsersTab : TabBar
{
	static readonly PackedScene PNodeBasePS = ResourceLoader.Load("res://pnodes/PNodeBase.tscn") as PackedScene;
	
	#region Nodes
	
	public GraphEdit GraphEditNode { get; private set; }
	public ItemList ParsersListNode { get; private set; }
	
	#endregion

	private Dictionary<PNode, List<string>> _unparcedTextIndex = new();
	public Dictionary<PNode, List<string>> UnparcedTextIndex { 
		get => _unparcedTextIndex;
		set {
			_unparcedTextIndex = value;
			
			// update existing graph nodes
			foreach (var child in GraphEditNode.GetChildren()) {
				var pNodeN = child as PNodeBase;
				var pNodeW = pNodeN.Data;
				
				if (!UnparcedTextIndex.ContainsKey(pNodeW.Value)) {
					continue;
				}
				
				var list = UnparcedTextIndex[pNodeW.Value];
				var children = pNodeN.GetChildren();
				var last = children[children.Count-1];
				UnprocessedTextList listNode;
				if (last is UnprocessedTextList) {
					listNode = last as UnprocessedTextList;
				} else {
					listNode = UnprocessedTextListPS.Instantiate() as UnprocessedTextList;
					pNodeN.AddChild(listNode);
				}
				listNode.Load(list);
			}
		}
	}
	
	public override void _Ready()
	{
		#region Node fetching
		
		GraphEditNode = GetNode<GraphEdit>("%GraphEdit");
		ParsersListNode = GetNode<ItemList>("%ParsersList");
		
		#endregion
		
	}
	
	private PNodeBase GetPNode(StringName name) => GraphEditNode.GetNode<PNodeBase>(name.ToString());
	
	private void OnGraphEditConnectionRequest(StringName from_node, int from_port, StringName to_node, int to_port)
	{
		var fromNode = GetPNode(from_node);
		var toNode = GetPNode(to_node);
		var fromPNode = fromNode.Data.Value;
		var toPNode = toNode.Data.Value;
		fromPNode.Children[from_port] = toPNode;
		// TODO not tested - don't know how safe it is to manually remove the last connection
		
		var removeQueue = new List<Godot.Collections.Dictionary>();
		// TODO remove previous connections
		foreach (var con in GraphEditNode.GetConnectionList()) {
			var fN = con["from"].As<string>();
			var fP = con["from_port"].As<int>();
			var tN = con["to"].As<string>();
			var tP = con["to_port"].As<int>();
			
			// check that the target port is not already taken
			if (tN == toNode.Name && tP == to_port) return;
			
			// check other connections from port
			if (fN == fromNode.Name && from_port == fP) {
				removeQueue.Add(con);
			}
			
		}
		foreach (var rD in removeQueue) {
			var fN = rD["from"].As<string>();
			var fP = rD["from_port"].As<int>();
			var tN = rD["to"].As<string>();
			var tP = rD["to_port"].As<int>();
			GraphEditNode.DisconnectNode(fN, fP, tN, tP);
			
		}

		GraphEditNode.ConnectNode(from_node, from_port, to_node, to_port);
	}
	
	private void OnGraphEditDisconnectionRequest(StringName from_node, int from_port, StringName to_node, int to_port)
	{
		var fromNode = GetPNode(from_node);
		var toNode = GetPNode(to_node);
		var fromPNode = fromNode.Data.Value;
		var toPNode = toNode.Data.Value;
		
		var i = fromNode.Data.GetChildIndex(toPNode);
		fromPNode.Children[i] = null;
		// TODO not tested - don't know how safe it is, especially for saving
		
		GraphEditNode.DisconnectNode(from_node, from_port, to_node, to_port);
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
		
		var pNodeW = ParsersListNode.GetItemMetadata(index).As<PNodeWrapper>();
		var pNode = pNodeW.Value;
		GraphEditNode.ClearConnections();
		
		foreach (var child in GraphEditNode.GetChildren())
			child.QueueFree();

		AddPNodeBase(pNodeW, true);
		GraphEditNode.ArrangeNodes();
	}

	static readonly PackedScene UnprocessedTextListPS = ResourceLoader.Load("res://UnprocessedTextList.tscn") as PackedScene;
	
	private PNodeBase AddPNodeBase(PNodeWrapper pNodeW, bool ignoreTemplate=false) {
		var node = PNodeBasePS.Instantiate() as PNodeBase;
		
		GraphEditNode.AddChild(node);
		node.Load(pNodeW, ignoreTemplate);
		
		// fill the unprocessed text
		if (UnparcedTextIndex.ContainsKey(pNodeW.Value)) {
			var list = UnparcedTextIndex[pNodeW.Value];
			var child = UnprocessedTextListPS.Instantiate() as UnprocessedTextList;
			node.AddChild(child);
			child.Load(list);
		}
		
		if (pNodeW.Value.IsTemplate && !ignoreTemplate) return node;
		
		// iterate over the children
		var childI = 0;
		foreach (var child in pNodeW.Value.Children) {
			var cNode = AddPNodeBase(new(child));
			GraphEditNode.ConnectNode(node.Name, childI, cNode.Name, 0);
			++childI;
		}
		return node;
	}
	
	[Signal]
	public delegate void ParserAddedEventHandler(PNodeWrapper pNodeW);
	
	private void OnMainProjectLoaded(Wrapper<Project> projectW)
	{
		var parsers = projectW.Value.Parsers;
		foreach (var parser in parsers)
			EmitSignal(SignalName.ParserAdded, new PNodeWrapper(parser));
	}
	
	private void OnParserAdded(PNodeWrapper pNodeW)
	{
		var pnode = pNodeW.Value;
		var i = ParsersListNode.AddItem(pnode.Name);
		ParsersListNode.SetItemMetadata(i, pNodeW);
	}
	
	public List<PNode> BakedParsers {
		get {
			var result = new List<PNode>();
			
			for (int i = 0; i < ParsersListNode.ItemCount; i++) {
				var pNodeW = ParsersListNode.GetItemMetadata(i).As<PNodeWrapper>();
				var pNode = pNodeW.Value;
				result.Add(pNode);
			}
			
			return result;
		}
	}
	
	private void AddNode() {
		GD.Print("Amogus");
	}

	private void OnGraphEditGuiInput(InputEvent e)
	{
		if (e.IsActionPressed("add-node")) {
			AddNode();
		}
	}
}



