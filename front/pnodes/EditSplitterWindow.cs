using Godot;
using System;
using System.Collections.Generic;

using MtgCardParser;

public partial class EditSplitterWindow : Window
{
	private Splitter _operated;
	#region Nodes
	
	public SplitterEditor EditorNode { get; private set; }
	
	#endregion
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		#region Node fetching
		
		EditorNode = GetNode<SplitterEditor>("%SplitterEditor");
		
		#endregion
	}
	
	private List<string> _takenNames;
	public void Load(Splitter splitter, List<string> takenNames) {
		_takenNames = takenNames;
		_operated = splitter;
		
		Title = splitter.Name + " splitter editor";
		
		EditorNode.SpName = splitter.Name;
		EditorNode.SpPattern = splitter.PatternString;
	}
	
	[Signal]
	public delegate void SplitterUpdatedEventHandler(PNodeWrapper pNodeW, string oldName);
	private void OnSaveButtonPressed()
	{
		var baked = EditorNode.GetBakedPNode() as Splitter;
		var flag = GUtil.CheckNameTaken(baked.Name, _operated.Name, _takenNames);
		if (!flag) {
			GUtil.Alert(this, "Parser name " + baked.Name + " is already taken");
			return;
		}
		
		// check that can save
		var oldName = _operated.Name;
		
		_operated.Name = baked.Name;
		_operated.PatternString = baked.PatternString;
		
		EmitSignal(SignalName.SplitterUpdated, new PNodeWrapper(_operated), oldName);
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
