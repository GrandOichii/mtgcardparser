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
	
	public void Load(Wrapper<PNode> pNodeW, bool ignoreTemplate) {
		var pNode = pNodeW.Value;
		Clear();
		
		Title = pNode.Name;

		if (pNode.IsTemplate && !ignoreTemplate) {
			AddChild(new Control());
			SetSlot(0, true, 0, new Color(0, 0, 1, 1), false, 0, new Color(0, 1, 0, 1));
			return;
		}
		
		switch (pNode) {
		case Matcher matcher:
			Title += " (matcher)";
			var count = matcher.GroupCount;
			for (int i = 0; i < count; i++) {
				var c = new Control();
				c.CustomMinimumSize = new(0, 40);
				AddChild(c);

				SetSlot(i, i == 0, 0, new Color(0, 0, 1, 1), true, 0, new Color(0, 1, 0, 1));
			}
			break;
		case Selector selector:
			Title += " (selector)";
			
			for (int i = 0; i < selector.Children.Count; i++) {
				var c = new Control();
				c.CustomMinimumSize = new(0, 40);
				AddChild(c);
				SetSlot(i, i == 0, 0, new Color(0, 0, 1, 1), true, 0, new Color(0, 1, 0, 1));
			}
			break;
		}
	}
	
	public void Clear() {
		ClearAllSlots();
		foreach (var child in GetChildren())
			child.QueueFree();
	}

}
