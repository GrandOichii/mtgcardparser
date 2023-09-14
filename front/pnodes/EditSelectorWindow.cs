using Godot;
using System;
using System.Collections.Generic;

using MtgCardParser;

public partial class EditSelectorWindow : Window
{
	private Selector _operated;
	
	#region Nodes
	
	public SelectorEditor EditorNode { get; private set; }
	
	#endregion
	
	public override void _Ready()
	{
		#region Node fetching
		
		EditorNode = GetNode<SelectorEditor>("%SelectorEditor");
		
		#endregion
	}
	
	private List<string> _takenNames;
	public void Load(Selector selector, List<string> takenNames) {
		_takenNames = takenNames;
		_operated = selector;
		
		Title = selector.Name + " selector editor";
		
		EditorNode.SName = selector.Name;
		EditorNode.SChildCount = selector.Children.Count;
	}
	
	[Signal]
	public delegate void SelectorUpdatedEventHandler(PNodeWrapper pNodeW, string oldName);

	private void OnSaveButtonPressed()
	{
		var baked = EditorNode.GetBakedPNode() as Selector;
		var flag = GUtil.CheckNameTaken(baked.Name, _operated.Name, _takenNames);
		if (!flag) {
			GUtil.Alert(this, "Parser name " + baked.Name + " is already taken");
			return;
		}

		// check that can save
		var oldName = _operated.Name;
		
		_operated.Name = baked.Name;
		if (_operated.Children.Count < baked.Children.Count) {
			for (int i = 0; i < baked.Children.Count - _operated.Children.Count; i++)
				_operated.Children.Add(null);
		}
		
		EmitSignal(SignalName.SelectorUpdated, new PNodeWrapper(_operated), oldName);
		Hide();
	}
	
	private void OnCancelButtonPressed()
	{
		Hide();
	}
	
	private void OnCloseRequested()
	{
		OnCancelButtonPressed();
	}
}
