using Godot;
using System;
using System.Collections.Generic;

using MtgCardParser;

public partial class TTTemplateEditor : Window
{
	[Signal]
	public delegate void TTTemplateAddedEventHandler(TTTemplateWrapper templateW);
	[Signal]
	public delegate void TTTemplateUpdatedEventHandler(TTTemplateWrapper newTemplateW, string oldTemplateName);
	
	public bool EditMode { get; private set; }
	public TTTemplateWrapper Last { get; private set; }
	public List<string> TemplateNames { get; set; }
	
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
		
		EditMode = ttTemplateW is not null;
		if (EditMode) {
			template = ttTemplateW?.Value;
			Last = ttTemplateW;
		}
		else {
			template = new();
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
		var name = NameEditNode.Text;
		if (name.Length == 0) {
			// TODO notify that can't save without name
			return;
		}
		
		// check that a template with the same name doesn't already exist
		var allowed = 0;
		if (EditMode && Last.Value.Name == name) ++allowed;
		var current = 0;
		foreach (var tName in TemplateNames) {
			if (tName != name) continue;
			
			++current;
			if (current > allowed) {
				// TODO notify the user that can't add
				return;
			}
		}
		
		var script = ScriptEditNode.Text;
		if (script.Length == 0) {
			// TODO notify that can't save without script
			return;
		}
		// TODO check that the script has a function Transform(text, card, args)
		var created = new LuaTextTransformerTemplate();
		created.Name = name;
		created.Script = script;
		
		var w = new TTTemplateWrapper(created);
		if (EditMode) {
			EmitSignal(SignalName.TTTemplateUpdated, w, Last.Value.Name);
			Hide();
			return;
		}
		EmitSignal(SignalName.TTTemplateAdded, w);
		Hide();
	}
	
	#endregion
}

public partial class TTArgWrapper : Node {
	public TTArg Value { get; }
	public TTArgWrapper(TTArg v) { Value = v; }
}


