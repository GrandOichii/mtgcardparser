using Godot;
using System;

using MtgCardParser;

public partial class MatcherEditor : Control, PNodeEditor
{
	#region Nodes
	
	public LineEdit NameEditNode { get; private set; }
	public LineEdit PatternEditNode { get; private set; }
	
	#endregion
	
	public override void _Ready()
	{
		#region Node fetching
		
		NameEditNode = GetNode<LineEdit>("%NameEdit");
		PatternEditNode = GetNode<LineEdit>("%PatternEdit");
		
		#endregion
	}
	
	public string MName {
		get => NameEditNode.Text;
		set => NameEditNode.Text = value;
	}
	
	public string MPattern {
		get => PatternEditNode.Text;
		set => PatternEditNode.Text = value;
	}
	
	public void ClearValues() {
		MName = "";
		MPattern = "";
	}

	public PNode GetBakedPNode() {
		var result = new Matcher();

		result.Name = MName;
		result.PatternString = MPattern;
		result.Children = new(new PNode[result.GroupCount]);
		for (int i = 0; i < result.GroupCount; i++)
			result.Children[i] = null;

		return result;
	}

	public void SetVisible(bool v) { Visible = v; }
	public bool IsVisible() => Visible;

}
