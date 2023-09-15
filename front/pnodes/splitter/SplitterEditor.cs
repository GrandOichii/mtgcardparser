using Godot;
using System;

using MtgCardParser;

public partial class SplitterEditor : Control, PNodeEditor
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
	
	public string SpName {
		get => NameEditNode.Text;
		set => NameEditNode.Text = value;
	}
	
	public string SpPattern {
		get => PatternEditNode.Text;
		set => PatternEditNode.Text = value;
	}
	
	public void ClearValues() {
		SpName = "";
		SpPattern = "";
	}

	public PNode GetBakedPNode() {
		var result = new Splitter();

		result.Name = SpName;
		result.PatternString = SpPattern;
		result.Children = new();
		result.Children.Add(null);

		return result;
	}

	public void SetVisible(bool v) { Visible = v; }
	public bool IsVisible() => Visible;
}
