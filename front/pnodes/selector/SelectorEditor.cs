using Godot;
using System;

using MtgCardParser;

public partial class SelectorEditor : Control, PNodeEditor
{
	#region Nodes
	
	public LineEdit NameEditNode { get; private set; }
	public SpinBox ChildSpinBoxNode { get; private set; }
	
	#endregion
	
	public override void _Ready()
	{
		#region Node fetching
		
		NameEditNode = GetNode<LineEdit>("%NameEdit");
		ChildSpinBoxNode = GetNode<SpinBox>("%ChildSpinBox");
		
		
		#endregion
	}
	
	public string SName {
		get => NameEditNode.Text;
		set => NameEditNode.Text = value;
	}
	
	public int SChildCount {
		get => (int)ChildSpinBoxNode.Value;
		set => ChildSpinBoxNode.Value = value;
	}
	
	public void ClearValues() {
		SName = "";
		SChildCount = 0;
	}
	
	public PNode GetBakedPNode() {
		var result = new Selector();

		result.Name = SName;
		result.Children = new(new PNode[SChildCount]);

		return result;
	}

	public void SetVisible(bool v) { Visible = v; }
	public bool IsVisible() => Visible;

}
