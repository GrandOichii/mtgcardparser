using Godot;
using System;

using MtgCardParser;


public partial class PNodeBase : GraphNode
{
	#region Nodes

	
	#endregion
	
	public override void _Ready()
	{
		#region Node fetching
		
		
		#endregion
		
		SetSlot(0, true, 0, new Color(1, 1, 1, 1), true, 0, new Color(0, 1, 0, 1));
	}
	
	public void Load(Wrapper<PNode> pNodeW) {
		var pNode = pNodeW.Value;
		Clear();
		
		Title = pNode.Name;
		switch (pNode) {
		case Matcher matcher:
			var count = matcher.GroupCount;
			for (int i = 0; i < count; i++) {
				AddChild(new Control());
				SetSlot(i, i == 0, 0, new Color(0, 0, 1, 1), true, 0, new Color(0, 1, 0, 1));
			}
			GD.Print(matcher.Name);
			GD.Print(GetChildren().Count);

			break;
		case Selector selector:
			AddChild(new Control());
			SetSlot(0, true, 0, new Color(0, 0, 1, 1), true, 0, new Color(0, 1, 0, 1));
			break;
		}
	}
	
	public void Clear() {
		ClearAllSlots();
		foreach (var child in GetChildren())
			child.QueueFree();
	}

}
