using Godot;
using System;
using System.Collections.Generic;

using MtgCardParser;


public partial class ParsersTab : TabBar
{
	static readonly PackedScene PNodeBasePS = ResourceLoader.Load("res://pnodes/PNodeBase.tscn") as PackedScene;
	
	#region Nodes
	
	public GraphEdit GraphEditNode { get; private set; }
	public ItemList ParsersListNode { get; private set; }
	public PopupMenu AddNodePopupMenuNode { get; private set; }
	public AddNodeWindow AddNodeWindowNode { get; private set; }
	public EditMatcherWindow EditMatcherWindowNode { get; private set; }
	public EditSelectorWindow EditSelectorWindowNode { get; private set; }	
	
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
		AddNodePopupMenuNode = GetNode<PopupMenu>("%AddNodePopupMenu");
		AddNodeWindowNode = GetNode<AddNodeWindow>("%AddNodeWindow");
		EditMatcherWindowNode = GetNode<EditMatcherWindow>("%EditMatcherWindow"); 
		EditSelectorWindowNode = GetNode<EditSelectorWindow>("%EditSelectorWindow");
		
		#endregion
		
	}
	
	private PNodeBase GetPNode(StringName name) => GraphEditNode.GetNode<PNodeBase>(name.ToString());
	
	private void OnGraphEditConnectionRequest(StringName from_node, int from_port, StringName to_node, int to_port)
	{
		var fromNode = GetPNode(from_node);
		var toNode = GetPNode(to_node);
		var fromPNode = fromNode.Data.Value;
		var toPNode = toNode.Data.Value;
		
		var removeQueue = new List<Godot.Collections.Dictionary>();

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

		fromPNode.Children[from_port] = toPNode;
		GraphEditNode.ConnectNode(from_node, from_port, to_node, to_port);
	}
	
	private void OnGraphEditDisconnectionRequest(StringName from_node, int from_port, StringName to_node, int to_port)
	{
		var fromNode = GetPNode(from_node);
		var toNode = GetPNode(to_node);
		var fromPNode = fromNode.Data.Value;
		var toPNode = toNode.Data.Value;
		
		fromPNode.Children[from_port] = null;
		
		GraphEditNode.DisconnectNode(from_node, from_port, to_node, to_port);
	}

	#region Button listeners
	
	private void OnAddButtonPressed()
	{
		AddNodeWindowNode.Do(BakedParserNames, true);
	}

	private void OnRemoveButtonPressed()
	{
		// TODO
	}
	
	#endregion
	
	private void OnTemplatesListItemActivated(int index)
	{
		var pNodeW = ParsersListNode.GetItemMetadata(index).As<PNodeWrapper>();
		var pNode = pNodeW.Value;
		GraphEditNode.ClearConnections();
		
		foreach (var child in GraphEditNode.GetChildren())
			child.Free();

		AddPNodeBase(pNodeW, true);
		GraphEditNode.ArrangeNodes();

		// center camera on the whole graph
		bool fuse = true;
		float minTop = 0;
		float minLeft = 0;
		float maxRight = 0;
		float maxBottom = 0;
		foreach (PNodeBase node in GraphEditNode.GetChildren()) {
			var pos = node.PositionOffset;
			var x = pos.X;
			var y = pos.Y;
			var mX = x + node.Size.X;
			var mY = y + node.Size.Y;

			if (fuse) {
				minTop = y;
				maxBottom = mY;
				minLeft = x;
				maxRight = mX;
				fuse = false;
				continue;
			}

			if (y < minTop) minTop = y;
			if (mY > maxBottom) maxBottom = mY;
			if (x < minLeft) minLeft = x;
			if (mX > maxRight) maxRight = mX;
		}
		
		var vX = GraphEditNode.Size.X;
		var vY = GraphEditNode.Size.Y;
		var rectX = maxRight - minLeft;
		var rectY = maxBottom - minTop;
		var ratioX = rectX / vX;
		var ratioY = rectY / vY;

		var ratioMax = Math.Max(ratioX, ratioY);
		var ratioMin = Math.Min(ratioX, ratioY);
		var evaluator = ratioMax;
		GraphEditNode.Zoom = 1f;
		if (evaluator > 1) {
			GraphEditNode.Zoom = 1f / evaluator;
		}
		
//		GraphEditNode.ScrollOffset = new((vX - rectX) / 2, (vY - rectY) / 2);
		GraphEditNode.ScrollOffset = new(0, 0);
		GraphEditNode.GrabFocus();
		
		// reset selected node
		_selectedPNodeBase = null;
	}
	
	public override void _Process(double delta) {
		
	}

	static readonly PackedScene UnprocessedTextListPS = ResourceLoader.Load("res://UnprocessedTextList.tscn") as PackedScene;
	
	private PNodeBase AddPNodeBase(PNodeWrapper pNodeW, bool ignoreTemplate=false) {
		var node = PNodeBasePS.Instantiate() as PNodeBase;
		
		GraphEditNode.AddChild(node);
		node.PositionOffset = new(0, 0);
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
			if (child is null) continue;
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
		// add parser to parser list
		var pnode = pNodeW.Value;
		var i = ParsersListNode.AddItem(pnode.Name);
		ParsersListNode.SetItemMetadata(i, pNodeW);

		// add parser to context menu
		AddNodePopupMenuNode.AddItem(pnode.Name + " (" + pnode.NodeName + ")");
		AddNodePopupMenuNode.SetItemMetadata(AddNodePopupMenuNode.ItemCount - 1, pNodeW);
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
	
	public List<string> BakedParserNames {
		get {
			var result = new List<string>();
			foreach (var parser in BakedParsers)
				result.Add(parser.Name);
			return result;
		}
	}
	
	public Vector2 LastMouseLocalPos { get; private set; }
	
	private void AddNode(Vector2 pos) {
		var mousePos = GetGlobalMousePosition();
		AddNodePopupMenuNode.PopupOnParent(new Rect2I((int)mousePos.X, (int)mousePos.Y, -1, -1));
		LastMouseLocalPos = GraphEditNode.GetLocalMousePosition();
	}

	private void OnGraphEditGuiInput(InputEvent e)
	{
		if (e.IsActionPressed("add-node")) {
			AddNode((e as InputEventMouseButton).Position);
		}
		if (e.IsActionPressed("edit-local-node")) {
			EditLocalNode();
		}
	}
	
	private void OnAddNodePopupMenuIdPressed(int id)
	{
		if (id < 2) {
			AddNodeWindowNode.Do(BakedParserNames);
			return;
		}
		
		var pNodeW = AddNodePopupMenuNode.GetItemMetadata(id).As<PNodeWrapper>();
		var child = AddPNodeBase(pNodeW);
		child.PositionOffset = LastMouseLocalPos;
	}
	
	private void OnAddNodeWindowPNodeCreated(PNodeWrapper pNodeW)
	{
		if (pNodeW.Value.IsTemplate) {

			EmitSignal(SignalName.ParserAdded, pNodeW);
			var i = ParsersListNode.ItemCount - 1;
			ParsersListNode.Select(i);
			OnTemplatesListItemActivated(i);
			return;
		}

		// is adding node to current graph edit
		var child = AddPNodeBase(pNodeW);
		child.PositionOffset = LastMouseLocalPos;
	}
	
	private Node? _selectedPNodeBase = null;
	private void OnGraphEditNodeSelected(Node node)
	{
		_selectedPNodeBase = node;
	}
	
	private void EditLocalNode() {
		if (_selectedPNodeBase is null) return;
		
		var pNodeN = _selectedPNodeBase as PNodeBase;
		var pNode = pNodeN.Data.Value;
		if (!pNodeN.IgnoreTemplate && pNode.IsTemplate) return;
		
		switch (pNode) {
		case Matcher matcher:
			EditMatcherWindowNode.Load(matcher);
			EditMatcherWindowNode.Show();
			break;
		case Selector selector:
			EditSelectorWindowNode.Load(selector);
			EditSelectorWindowNode.Show();
			break;
		}
	}
	
	private void PNodeUpdated(PNodeWrapper pNodeW, string oldName) {
		
		// replace all present nodes
		foreach (var child in GraphEditNode.GetChildren()) {
			var pNodeB = child as PNodeBase;
			if (pNodeB.Data.Value.Name != pNodeW.Value.Name) continue;
			
//			pNodeB.Data.Value.Name = newName;
			pNodeB.Load(pNodeW, pNodeB.IgnoreTemplate);
		}
		
		var newName = pNodeW.Value.Name;
		if (pNodeW.Value.IsTemplate) {
			// replace the list name
			ReplaceParsersListParserName(oldName, newName);
		}
		
		foreach (var child in pNodeW.Value.Children)
			if (child is not null) return;
		
		var removeQueue = new List<Godot.Collections.Dictionary>();
		
		// disconnect all connections
		foreach (var con in GraphEditNode.GetConnectionList()) {
			var fN = con["from"].As<string>();
			var fP = con["from_port"].As<int>();
			var tN = con["to"].As<string>();
			var tP = con["to_port"].As<int>();
			
			// check other connections from port
			var fromNode = GraphEditNode.GetNode(fN) as PNodeBase;
			if (fromNode.Data.Value.Name == newName) {
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
	}

	private void OnEditMatcherWindowMatcherUpdated(PNodeWrapper pNodeW, string oldName)
	{
		PNodeUpdated(pNodeW, oldName);
	}
	
	private void ReplaceParsersListParserName(string oldName, string newName) {
		for (int i = 0; i < ParsersListNode.ItemCount; i++) {
			if (ParsersListNode.GetItemText(i) != oldName) continue;
			
			ParsersListNode.SetItemText(i, newName);
			ParsersListNode.GetItemMetadata(i).As<PNodeWrapper>().Value.Name = newName;
			return;
		}
		throw new Exception("Failed to replace parser name in list: " + oldName + " -> " + newName);
	}
	
	private void OnEditSelectorWindowSelectorUpdated(PNodeWrapper pNodeW, string oldName)
	{
		PNodeUpdated(pNodeW, oldName);
	}
}
