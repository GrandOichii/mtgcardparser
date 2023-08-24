using Godot;
using System;

using MtgCardParser;

public partial class TTTemplateEditor : Window
{
	[Signal]
	public delegate void TTTemplateAddedEventHandler(TTTemplateWrapper templateW);
	[Signal]
	public delegate void TTTemplateUpdatedEventHandler(TTTemplateWrapper templateW);
	
	public bool EditMode { get; private set; }
	
	#region Nodes
	public LineEdit NameEditNode { get; private set; }
	public TextEdit DescriptionEditNode { get; private set; }
	public ItemList ArgumentListNode { get; private set; }
	public LineEdit NewArgumentEditNode { get; private set; }
	public CodeEdit ScriptEditNode { get; private set; }
	#endregion
	
	public override void _Ready()
	{
		#region Node fetching
		
		NameEditNode = GetNode<LineEdit>("%NameEdit");
		DescriptionEditNode = GetNode<TextEdit>("%DescriptionEdit");
		ArgumentListNode = GetNode<ItemList>("%ArgumentList");
		NewArgumentEditNode = GetNode<LineEdit>("%NewArgumentEdit");
		ScriptEditNode = GetNode<CodeEdit>("%ScriptEdit");

		#endregion
	}

	public void Load(TTTemplateWrapper? ttTemplateW) {
		LuaTextTransformerTemplate template;
		
		EditMode = ttTemplateW is null;
		if (EditMode) {
			template = new();
		}
		else {
			template = ttTemplateW?.Value;
		}
		
		NameEditNode.Text = template.Name;
		DescriptionEditNode.Text = template.Description;
		ScriptEditNode.Text = template.Script;
		
		// arguments
		foreach (var arg in template.Args) {
			var i = ArgumentListNode.AddItem(arg.Name);
			ArgumentListNode.SetItemMetadata(i, new TTArgWrapper(arg));
		}
	}
	
	#region Argument list
	
	private void OnArgumentListItemActivated(int index)
	{
		// TODO prompt user to remove
		ArgumentListNode.RemoveItem(index);
	}
	
	private void OnAddArgumentButtonPressed()
	{
		var argName = NewArgumentEditNode.Text;
		for (int i = 0; i < ArgumentListNode.ItemCount; i++) {
			var itemText = ArgumentListNode.GetItemText(i);
			if (itemText == argName) {
				// TODO notify the user that can't add the argument
				return;
			}
		}
		var index = ArgumentListNode.AddItem(argName);
		ArgumentListNode.SetItemMetadata(index, new TTArgWrapper(new(argName)));
		
		NewArgumentEditNode.Text = "";
	}
	
	#endregion
	
	#region Window closing
	
	private void OnCloseRequested()
	{
		Hide();
	}
		
	private void OnSaveButtonPressed()
	{
		
	}
	
	#endregion
}

public partial class TTArgWrapper : Node {
	public TTArg Value { get; }
	public TTArgWrapper(TTArg v) { Value = v; }
}


