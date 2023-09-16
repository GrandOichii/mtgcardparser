using Godot;
using System;
using System.Collections.Generic;

public partial class UnprocessedTextList : ScrollContainer
{
	#region Nodes
	
	public ItemList ListNode { get; private set; }
	
	#endregion
	public override void _Ready()
	{
		#region Node fetching
		
		ListNode = GetNode<ItemList>("%List");
		
		#endregion
	}

	public void Load(List<string> parcedList, List<string> unparcedList, Texture2D pTex, Texture2D uTex) {
		ListNode.Clear();
		foreach (var line in unparcedList) 
			ListNode.AddItem(line, uTex);
		foreach (var line in parcedList) 
			ListNode.AddItem(line, pTex);
	}
}
