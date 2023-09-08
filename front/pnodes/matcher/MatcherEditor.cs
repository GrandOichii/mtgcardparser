using Godot;
using System;

using MtgCardParser;

public partial class MatcherEditor : PNodeEditor
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
	
	public override void ClearValues() {
		MName = "";
		MPattern = "";
	}

	public override PNode GetBakedPNode() {
		var result = new Matcher();

		result.Name = MName;
		result.PatternString = MPattern;
		result.Children = new(new PNode[result.GroupCount]);

		return result;
	}

}
