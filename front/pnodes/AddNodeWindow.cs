using Godot;
using System;

using System.Collections.Generic;

public partial class AddNodeWindow : Window
{
	[Signal]
	public delegate void PNodeCreatedEventHandler(PNodeWrapper pNodeW);
	
	#region Nodes
	
	public MatcherEditor MatcherEditorNode { get; private set; }
	public SelectorEditor SelectorEditorNode { get; private set; }
	public OptionButton TypeOptionNode { get; private set; }
	
	#endregion
	
	public bool Saved { get; private set; } = false;
	
	public Dictionary<string, PNodeEditor> PNodeEditorMap { get; private set; }
	
	public override void _Ready()
	{
		#region Node fetching
		
		MatcherEditorNode = GetNode<MatcherEditor>("%MatcherEditor");
		SelectorEditorNode = GetNode<SelectorEditor>("%SelectorEditor");
		TypeOptionNode = GetNode<OptionButton>("%TypeOption");
		
		#endregion
		
		PNodeEditorMap = new() {
			{ "matcher", MatcherEditorNode },
			{ "selector", SelectorEditorNode }
		};
	}
	
	private void OnTypeOptionItemSelected(int index)
	{
		var option = TypeOptionNode.GetItemText(index);

		foreach (var pair in PNodeEditorMap) {
			var editor = pair.Value;
			editor.Visible = false;
			
			if (pair.Key != option) continue;
			editor.Visible = true;
			editor.ClearValues();
		}
	}
	
	private void OnCloseRequested()
	{
		Saved = false;
		Visible = false;
	}
	
	public void Do() {
		Saved = true;
		Visible = true;
	}
	
	private void OnCancelButtonPressed()
	{
		OnCloseRequested();
	}

	private void OnAddButtonPressed()
	{
		var sType = TypeOptionNode.GetItemText(TypeOptionNode.GetSelectedId()); 
		// check name
		// check pattern
		
		GD.Print(sType);
		// Replace with function body.
	}

}







