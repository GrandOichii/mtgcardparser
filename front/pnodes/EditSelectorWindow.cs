using Godot;
using System;

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
	
	public void Load(Selector selector) {
		_operated = selector;
		
		Title = selector.Name + " selector editor";
		
		EditorNode.SName = selector.Name;
		EditorNode.SChildCount = selector.Children.Count;
	}
	
	[Signal]
	public delegate void SelectorUpdatedEventHandler(PNodeWrapper pNodeW, string oldName);

	private void OnSaveButtonPressed()
	{
		// TODO check that name is not taken
		var baked = EditorNode.GetBakedPNode() as Selector;
		
		// check that can save
		var oldName = _operated.Name;
		
		_operated.Name = baked.Name;
		if (_operated.Children.Count != baked.Children.Count) {
			_operated.Children = baked.Children;
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
