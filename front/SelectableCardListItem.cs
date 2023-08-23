using Godot;
using System;

public partial class SelectableCardListItem : Button
{
	#region Nodes
	public CheckBox SelectedCheckBoxNode { get; private set; }
	public Label NameLabelNode { get; private set; }
	#endregion
	
	public override void _Ready() {
		#region Nodes fetching
		SelectedCheckBoxNode = GetNode<CheckBox>("%SelectedCheckBox");
		NameLabelNode = GetNode<Label>("%NameLabel");
		#endregion
	}
	
	public void Load(SourceCard card) {
		NameLabelNode.Text = card.Name;
	}
}
