using Godot;
using System;
using System.Collections.Generic;

using MtgCardParser;

public partial class AddNodeWindow : Window
{
	[Signal]
	public delegate void PNodeCreatedEventHandler(PNodeWrapper pNodeW);

	private List<string> _parserNames;
	
	#region Nodes
	
	public MatcherEditor MatcherEditorNode { get; private set; }
	public SelectorEditor SelectorEditorNode { get; private set; }
	public SplitterEditor SplitterEditorNode { get; private set; }
	public OptionButton TypeOptionNode { get; private set; }
	
	#endregion
		
	public Dictionary<string, PNodeEditor> PNodeEditorMap { get; private set; }
	
	public override void _Ready()
	{
		#region Node fetching
		
		MatcherEditorNode = GetNode<MatcherEditor>("%MatcherEditor");
		SplitterEditorNode = GetNode<SplitterEditor>("%SplitterEditor");
		SelectorEditorNode = GetNode<SelectorEditor>("%SelectorEditor");
		TypeOptionNode = GetNode<OptionButton>("%TypeOption");
		
		#endregion
		
		PNodeEditorMap = new() {
			{ "matcher", MatcherEditorNode },
			{ "splitter", SplitterEditorNode },
			{ "selector", SelectorEditorNode }
		};
	}

	private PNodeEditor CurrentEditor {
		get {
			foreach (var editor in PNodeEditorMap.Values)
				if (editor.IsVisible())
					return editor;
			throw new Exception("No editor visible in AddNodeWindow");
		}
	}
	
	private void OnTypeOptionItemSelected(int index)
	{
		var option = TypeOptionNode.GetItemText(index);

		foreach (var pair in PNodeEditorMap) {
			var editor = pair.Value;
			editor.SetVisible(false);
			
			if (pair.Key != option) continue;
			editor.SetVisible(true);
			editor.ClearValues();
		}
	}
	
	private void OnCloseRequested()
	{
		Visible = false;
	}

	private bool _isTemplate;
	
	public void Do(List<string> parserNames, bool isTemplate=false) {
		_isTemplate = isTemplate;
		Visible = true;
		_parserNames = parserNames;
		
		foreach (var editor in PNodeEditorMap.Values)
			editor.ClearValues();
		
	}
	
	private void OnCancelButtonPressed()
	{
		OnCloseRequested();
	}

	private void OnAddButtonPressed()
	{
		var sType = TypeOptionNode.GetItemText(TypeOptionNode.GetSelectedId()); 
		// check pattern
		PNode result;
		try {
			result = CurrentEditor.GetBakedPNode();
		} catch (Exception ex) {
			// TODO show user
			GD.Print(ex);
			return;
		}
		
		// check name
		 if (_isTemplate) {
			result.IsTemplate = _isTemplate;
			
			if (_parserNames.Contains(result.Name)) {
				GUtil.Alert(this, "Parser with name " + result.Name + " already exists");

				return;
			}
		 }

		EmitSignal(SignalName.PNodeCreated, new PNodeWrapper(result));
		Visible = false;
	}

}







