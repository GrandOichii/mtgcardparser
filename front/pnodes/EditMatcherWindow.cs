using Godot;
using System;
using System.Collections.Generic;

using MtgCardParser;

public partial class EditMatcherWindow : Window
{
	private Matcher _operated;
	#region Nodes
	
	public MatcherEditor EditorNode { get; private set; }
	
	#endregion
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		#region Node fetching
		
		EditorNode = GetNode<MatcherEditor>("%MatcherEditor");
		
		#endregion
	}
	
	private List<string> _takenNames;
	public void Load(Matcher matcher, List<string> takenNames) {
		_takenNames = takenNames;
		_operated = matcher;
		
		Title = matcher.Name + " matcher editor";
		
		EditorNode.MName = matcher.Name;
		EditorNode.MPattern = matcher.PatternString;
	}
	
	[Signal]
	public delegate void MatcherUpdatedEventHandler(PNodeWrapper pNodeW, string oldName);
	private void OnSaveButtonPressed()
	{
		var baked = EditorNode.GetBakedPNode() as Matcher;
		var flag = GUtil.CheckNameTaken(baked.Name, _operated.Name, _takenNames);
		if (!flag) {
			GUtil.Alert(this, "Parser name " + baked.Name + " is already taken");
			return;
		}
		
		// check that can save
		var oldName = _operated.Name;
		
		_operated.Name = baked.Name;
		if (_operated.GroupCount != baked.GroupCount) {
			_operated.Children = baked.Children;
		}
		_operated.PatternString = baked.PatternString;
		
		EmitSignal(SignalName.MatcherUpdated, new PNodeWrapper(_operated), oldName);
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






