using Godot;
using System;

using MtgCardParser;


public partial class PNodeBase : GraphNode
{
	static readonly Color LEFT_COLOR = new Color(1, 1, 0, 1);
	static readonly Color RIGHT_COLOR = new Color(1, 1, 0, 1);
	
	public PNodeWrapper Data { get; set; }
	
	#region Nodes

	
	#endregion
	
	public override void _Ready()
	{
		#region Node fetching
		
		
		#endregion
		
		SetSlot(0, true, 0, LEFT_COLOR, true, 0, RIGHT_COLOR);
	}
	
	public void Load(PNodeWrapper pNodeW, bool ignoreTemplate) {
		Data = pNodeW;
		
		var pNode = pNodeW.Value;
		Clear();
		
		Title = pNode.Name;

		var isTemplate = pNode.IsTemplate && !ignoreTemplate;
		if (isTemplate) {
			AddChild(new Control());
			SetSlot(0, true, 0, LEFT_COLOR, false, 0, RIGHT_COLOR);
		}
		
		switch (pNode) {
		case Matcher matcher:
			Title += " (matcher)";
			if (isTemplate) break;

			var count = matcher.GroupCount;
			for (int i = 0; i < count; i++) {
				var c = new Control();
				c.CustomMinimumSize = new(0, 40);
				AddChild(c);

				SetSlot(i, i == 0, 0, LEFT_COLOR, true, 0, RIGHT_COLOR);
			}
			break;
		case Selector selector:
			Title += " (selector)";
			if (isTemplate) break;
			
			for (int i = 0; i < selector.Children.Count; i++) {
				var c = new Control();
				c.CustomMinimumSize = new(0, 40);
				AddChild(c);
				SetSlot(i, i == 0, 0, LEFT_COLOR, true, 0, RIGHT_COLOR);
			}
			break;
		}


	}
	
	public void Clear() {
		ClearAllSlots();
		foreach (var child in GetChildren())
			child.QueueFree();
	}

	private void OnResizeRequest(Vector2 new_minsize)
	{
		Size = new_minsize;
	}
}


