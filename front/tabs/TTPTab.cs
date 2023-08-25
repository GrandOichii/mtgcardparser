using Godot;
using System;
using System.Collections.Generic;

using MtgCardParser;

// TODO? add scrolls to lists


//public partial class Wrapper<LuaTextTransformerTemplate> : Node {
//	public LuaTextTransformerTemplate Value { get; }
//	public TTTemplateWrapper(LuaTextTransformerTemplate v) { Value = v; }
//}
//
//public partial class TTWrapper : Node {
//
//}


public partial class TTPTab : TabBar
{
	#region Nodes
	public ItemList TextTransformerListNode { get; private set; }
	public ItemList TextTransformerTemplateListNode { get; private set; }
	public VBoxContainer TextBoxesNode { get; private set; }
	public TTTemplateEditor TTTemplateEditorNode { get; private set; }
	public TTEditor TTEditorNode { get; private set; }
	public TabContainer TTTabContainerNode { get; private set; }
	#endregion
	
	public override void _Ready() {
		#region Node fetching
		
		TextTransformerListNode = GetNode<ItemList>("%TextTransformerList");
		TextTransformerTemplateListNode = GetNode<ItemList>("%TextTransformerTemplateList");
		TextBoxesNode = GetNode<VBoxContainer>("%TextBoxes");
		TTTemplateEditorNode = GetNode<TTTemplateEditor>("%TTTemplateEditor");
		TTEditorNode = GetNode<TTEditor>("%TTEditor");
		TTTabContainerNode = GetNode<TabContainer>("%TTTabContainer");
		
		#endregion
	}
	
	#region Project loading
	
	private void OnProjectLoaded(Wrapper<Project> projectW)
	{
		var ttp = projectW.Value.TTPipeline;
		// load templates
		foreach (var template in ttp.CustomTemplates) {
			AddCustomTemplate(template);
		}
		
		// load pipeline
		foreach (var tt in ttp.Pipeline) {
			AddTT(tt);
		}
		
		RunPipeline();
	}
	
	private void AddCustomTemplate(LuaTextTransformerTemplate template) {
		AddCustomTemplate(new Wrapper<LuaTextTransformerTemplate>(template));
	}
	
	private void AddCustomTemplate(Wrapper<LuaTextTransformerTemplate> templateW) {
		var i = TextTransformerTemplateListNode.AddItem(templateW.Value.Name);
		TextTransformerTemplateListNode.SetItemMetadata(i, templateW);
	}
	
	private void AddTT(TextTransformer tt) {
		AddTT(new Wrapper<TextTransformer>(tt));
	}
	
	private void AddTT(Wrapper<TextTransformer> ttW) {
		var i = TextTransformerListNode.AddItem(ttW.Value.Name);
		TextTransformerListNode.SetItemMetadata(i, ttW);
	}
	
	#endregion

	#region Text Transformer Templates
	
	private void OnTextTransformerTemplateListItemActivated(int index)
	{
		var ttTemplateW = TextTransformerTemplateListNode.GetItemMetadata(index).As<Wrapper<LuaTextTransformerTemplate>>();
		TTTemplateEditorNode.TemplateNames = TemplateNames;
		TTTemplateEditorNode.Load(ttTemplateW);
		TTTemplateEditorNode.Show();
	}

	private void OnTTTemplateEditorTTTemplateAdded(Wrapper<LuaTextTransformerTemplate> templateW)
	{
		AddCustomTemplate(templateW);
	}

	private void OnTTTemplateEditorTTTemplateUpdated(Wrapper<LuaTextTransformerTemplate> newTemplateW, string oldTemplateName)
	{
		for (int i = 0; i < TextTransformerTemplateListNode.ItemCount; i++) {
			var itemText = TextTransformerTemplateListNode.GetItemText(i);
			if (itemText == oldTemplateName) {
				TextTransformerTemplateListNode.SetItemText(i, newTemplateW.Value.Name);
				TextTransformerTemplateListNode.SetItemMetadata(i, newTemplateW);
				return;
			}
		}
		GD.Print("WARN: tried to update text transformer template list item with name " + oldTemplateName + " to " + newTemplateW.Value.Name + ", but no such item exists");
	}


	#endregion
	
	[Signal]
	public delegate void TTTemplateDeletedEventHandler(Wrapper<LuaTextTransformerTemplate> templateW);
	
	private void OnAddButtonPressed()
	{
		// TODO ugly, think of smt other
		switch(TTTabContainerNode.CurrentTab) {
		case 0:
			// text transformers
			// TODO
			return;
		case 1:
			// text transformer templates
			TTTemplateEditorNode.TemplateNames = TemplateNames;
			TTTemplateEditorNode.Load(null);
			TTTemplateEditorNode.Show();
			return;
		}
	}
	
	private void OnEditButtonPressed()
	{
		// TODO ugly, think of smt other
		switch(TTTabContainerNode.CurrentTab) {
		case 0:
			// text transformers
			// TODO
			return;
		case 1:
			// text transformer templates
			
			var items = TextTransformerTemplateListNode.GetSelectedItems();
			if (items.Length == 0) {
				// TODO notify the user to select a template first
				return;
			}
			var data = TextTransformerTemplateListNode.GetItemMetadata(items[0]).As<Wrapper<LuaTextTransformerTemplate>>();
			TTTemplateEditorNode.TemplateNames = TemplateNames;
			TTTemplateEditorNode.Load(data);
			TTTemplateEditorNode.Show();
			return;
		}
	}
	
	private void OnRemoveButtonPressed()
	{
		switch(TTTabContainerNode.CurrentTab) {
		case 0:
			// text transformers
			// TODO
			return;
		case 1:
			// text transformer templates
			
			var items = TextTransformerTemplateListNode.GetSelectedItems();
			if (items.Length == 0) {
				// TODO notify the user to select a template first
				return;
			}
			// TODO ask the user to confirm deleting the templates
			var data = TextTransformerTemplateListNode.GetItemMetadata(items[0]);
			TextTransformerTemplateListNode.RemoveItem(items[0]);
			EmitSignal(SignalName.TTTemplateDeleted, data);
			return;
		}
	}
	
	private List<string> TemplateNames {
		get {
			var result = new List<string>();
			for (int i = 0; i < TextTransformerTemplateListNode.ItemCount; i++) {
				var itemText = TextTransformerTemplateListNode.GetItemText(i);
				result.Add(itemText);
			}
			return result;
		}
	}
	
	private List<string> TTNames {
		get {
			var result = new List<string>();
			for (int i = 0; i < TextTransformerListNode.ItemCount; i++) {
				var itemText = TextTransformerListNode.GetItemText(i);
				result.Add(itemText);
			}
			return result;
		}
	}
	
	#region Text transformers
	
	private void OnTextTransformerListItemActivated(int index)
	{
		var ttW = TextTransformerListNode.GetItemMetadata(index).As<Wrapper<TextTransformer>>();
		TTEditorNode.TTNames = TTNames;
		TTEditorNode.Load(ttW);
		TTEditorNode.Show();
	}

	private void OnTTEditorTTAdded(Wrapper<TextTransformer> ttW)
	{
		AddTT(ttW);
		RunPipeline();
	}

	private void OnTTEditorTTUpdated(Wrapper<TextTransformer> newTTW, string oldName)
	{
		for (int i = 0; i < TextTransformerListNode.ItemCount; i++) {
			var itemText = TextTransformerListNode.GetItemText(i);
			if (itemText == oldName) {
				TextTransformerListNode.SetItemText(i, newTTW.Value.Name);
				TextTransformerListNode.SetItemMetadata(i, newTTW);
				RunPipeline();
				return;
			}
		}
		GD.Print("WARN: tried to update text transformer list item with name " + oldName + " to " + newTTW.Value.Name + ", but no such item exists");

	}

	private void OnMoveTTUpButtonPressed()
	{
		MoveSelectedTTLI(-1);
	}

	private void OnMoveTTDownButtonPressed()
	{
		MoveSelectedTTLI(1);
	}
	
	private void MoveSelectedTTLI(int amount) {
		var items = TextTransformerListNode.GetSelectedItems();
		if (items.Length == 0) {
			// TODO notify the user to select a template first
			return;
		}
		
		var fromI = items[0];
		var toI = items[0] + amount;
		
		// TODO? wrap around
		if (toI < 0 || toI >= TextTransformerListNode.ItemCount)
			return;
		
		TextTransformerListNode.MoveItem(fromI, toI);
		RunPipeline();
	}
	
	#endregion
	
	#region Pipeline activation
	
	public TextTransformerPipeline BakedPipeline {
		get {
			var result = new TextTransformerPipeline();
			
			// custom templates
			for (int i = 0; i < TextTransformerTemplateListNode.ItemCount; i++) {
				var template = TextTransformerTemplateListNode.GetItemMetadata(i).As<Wrapper<LuaTextTransformerTemplate>>().Value;
				result.CustomTemplates.Add(template);

				template.LState = result.LState;
				template.Script = template.Script;
			}
			
			// pipeline
			for (int i = 0; i < TextTransformerListNode.ItemCount; i++) {
				var tt = TextTransformerListNode.GetItemMetadata(i).As<Wrapper<TextTransformer>>().Value;
				result.Pipeline.Add(tt);
			}
			
			return result;
		}
	}
	
	[Signal]
	public delegate void RunTTPipelineEventHandler();
	
	public void RunPipeline() {
		EmitSignal(SignalName.RunTTPipeline);
	}
	
	#endregion

	private void OnCardsListCardClicked(SourceCard card)
	{
//		EmitSignal(SignalName.TransformCardText, card);
		var pipeline = BakedPipeline;
		var result = pipeline.DoDetailed(card.ToCard());
		// foreach (var stage in result)
		// 	GD.Print(stage);
		
		SetTTResultLayers(result);
	}

	static readonly PackedScene TTResultLayerPS = ResourceLoader.Load("res://TTResultLayer.tscn") as PackedScene;
//	static readonly PackedScene TTResultLayerSeparatorPS = ResourceLoader.Load("res://TTResultLayerSeparator.cs") as PackedScene;
	
	private void SetTTResultLayers(List<string> texts) {
		foreach (var child in TextBoxesNode.GetChildren())
			child.QueueFree();
		for (int i = 0; i < texts.Count; i++)
			AddTTResultLayer(i, texts);
	}
	
	private void AddTTResultLayer(int i, List<string> texts) {
		bool addSeparator = i == 0;
		string name = "Base text";
		if (!addSeparator) name = TextTransformerListNode.GetItemText(i-1);
		
		var child = TTResultLayerPS.Instantiate() as TTResultLayer;
		TextBoxesNode.AddChild(child);
		child.Load(name, texts[i]);
	}

}






