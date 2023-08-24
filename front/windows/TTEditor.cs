using Godot;
using System;
using System.Collections.Generic;

using MtgCardParser;

// TODO add signal handlers for updating and removing templates

public partial class TTEditor : Window
{
	static readonly PackedScene TTArgListItemPS = ResourceLoader.Load("res://windows/TTArgListItem.tscn") as PackedScene;

	public List<string> TTNames { get; set; }
	private bool _editMode;
	private Wrapper<TextTransformer> _last;
	
	private List<LuaTextTransformerTemplate> _templates=new();
	
	#region Nodes
	
	public LineEdit NameEditNode { get; private set; }
	public OptionButton TemplateOptionNode { get; private set; }
	public VBoxContainer ArgumentListNode { get; private set; }
	
	#endregion
	
	public override void _Ready()
	{
		#region Node fetching
		
		NameEditNode = GetNode<LineEdit>("%NameEdit");
		TemplateOptionNode = GetNode<OptionButton>("%TemplateOption");
		ArgumentListNode = GetNode<VBoxContainer>("%ArgumentList");
		
		#endregion
	}
	
	#region TT loading
	
	public void Load(Wrapper<TextTransformer>? ttW) {
		TextTransformer tt;
		_editMode = ttW is not null;
		
		if (_editMode) {
			_last = ttW;
			tt = ttW?.Value;
		} else {
			tt = new TextTransformer();
		}
		
		NameEditNode.Text = tt.Name;
		if (tt.Template is not null)
			TemplateOptionNode.Selected = IndexOf(tt.Template?.Name);
			
		// template arguments
		ClearArgumentList();
		// TODO add only arguments associated with the template
		foreach (var pair in tt.TemplateArgs) {
			var argName = pair.Key;
			var argValue = pair.Value;
			var child = TTArgListItemPS.Instantiate() as TTArgListItem;
			ArgumentListNode.AddChild(child);
			child.Load(argName, argValue);
		}
	}
	
	private void ClearArgumentList() {
		foreach (var child in ArgumentListNode.GetChildren())
			child.QueueFree();
	}
	
	#endregion
	
	public int IndexOf(string item) {
		for (int i = 0; i < TemplateOptionNode.ItemCount; i++)
			if (item == TemplateOptionNode.GetItemText(i))
				return i;
		return 0;
	}
	
	#region Text transformer templates
	
	private void OnTTTemplateEditorTTTemplateAdded(Wrapper<LuaTextTransformerTemplate> templateW)
	{
		TemplateOptionNode.AddItem(templateW.Value.Name);
		var index = IndexOf(templateW.Value.Name);
		TemplateOptionNode.SetItemMetadata(index, templateW);
	}
	
	#endregion
	
	private void OnCloseRequested()
	{
		Hide();
	}

	#region Project loading
	
	private void OnMainProjectLoaded(Wrapper<Project> projectW)
	{
		foreach (var template in projectW.Value.TTPipeline.Templates) {
			TemplateOptionNode.AddItem(template.Name);
			var index = IndexOf(template.Name);
			TemplateOptionNode.SetItemMetadata(index, new Wrapper<TextTransformerTemplate>(template));
		}
		foreach (var template in projectW.Value.TTPipeline.CustomTemplates) {
			TemplateOptionNode.AddItem(template.Name);
			var index = IndexOf(template.Name);
			TemplateOptionNode.SetItemMetadata(index, new Wrapper<TextTransformerTemplate>(template));
		}
	}
	
	#endregion
	
	[Signal]
	public delegate void TTAddedEventHandler(Wrapper<TextTransformer> ttW);
	[Signal]
	public delegate void TTUpdatedEventHandler(Wrapper<TextTransformer> newTTW, string oldName);
	
	private void OnSaveButtonPressed()
	{
		var name = NameEditNode.Text;
		
		if (name.Length == 0) {
			// TODO notify that can't save without name
			return;
		}
		
		// check that a template with the same name doesn't already exist
		var allowed = 0;
		if (_editMode && _last.Value.Name == name) ++allowed;
		var current = 0;
		foreach (var ttName in TTNames) {
			if (ttName != name) continue;

			++current;
			if (current > allowed) {
				// TODO notify the user that can't add
				return;
			}
		}
		
		var templateI = TemplateOptionNode.Selected;
		if (templateI == -1) {
			// TODO notify user to pick template first
			return;
		}
		
		var created = new TextTransformer();
		created.Name = name;
		created.Template = TemplateOptionNode.GetItemMetadata(templateI).As<Wrapper<TextTransformerTemplate>>().Value;
		foreach (var childNode in ArgumentListNode.GetChildren()) {
			var child = childNode as TTArgListItem;
			created.TemplateArgs.Add(
				child.ArgNameLabelNode.Text,
				child.ArgValueEditNode.Text
			);
		}
		var w = new Wrapper<TextTransformer>(created);
		if (_editMode) {
			EmitSignal(SignalName.TTUpdated, w, _last.Value.Name);
			Hide();
			return;
		}
		EmitSignal(SignalName.TTAdded, w);
		Hide();
	}
}


