using Godot;
using System;
using System.Collections.Generic;

public partial class UnprocessedTextList : ScrollContainer
{
	#region Nodes
	
	public ItemList ListNode { get; private set; }
	
	#endregion
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		#region Node fetching
		
		ListNode = GetNode<ItemList>("%List");
		
		#endregion
	}

	public void Load(List<string> list) {
		ListNode.Clear();
		foreach (var line in list)
			ListNode.AddItem(line);
	}
}
