using Godot;
using System;

using MtgCardParser;

// TODO? add scrolls to lists


public partial class TTTemplateWrapper : Node {
	public LuaTextTransformerTemplate Value { get; }
	public TTTemplateWrapper(LuaTextTransformerTemplate v) { Value = v; }
}


public partial class TPPTab : TabBar
{
	#region Nodes
	public ItemList TextTransformerListNode { get; private set; }
	public ItemList TextTransformerTemplateListNode { get; private set; }
	public VBoxContainer TextBoxesNode { get; private set; }
	public TTTemplateEditor TTTemplateEditorNode { get; private set; }
	public TabContainer TTTabContainerNode { get; private set; }
	#endregion
	
	public override void _Ready() {
		#region Node fetching
		
		TextTransformerListNode = GetNode<ItemList>("%TextTransformerList");
		TextTransformerTemplateListNode = GetNode<ItemList>("%TextTransformerTemplateList");
		TextBoxesNode = GetNode<VBoxContainer>("%TextBoxes");
		TTTemplateEditorNode = GetNode<TTTemplateEditor>("%TTTemplateEditor");
		TTTabContainerNode = GetNode<TabContainer>("%TTTabContainer");
		
		#endregion
	}
	
	#region Project loading
	
	private void OnProjectLoaded(ProjectWrapper project)
	{
		var ttp = project.Project.TTPipeline;
		// load templates
		foreach (var template in ttp.CustomTemplates) {
			AddCustomTemplate(template);
		}
	}
	
	private void AddCustomTemplate(LuaTextTransformerTemplate template) {
		AddCustomTemplate(new TTTemplateWrapper(template));
	}
	
	private void AddCustomTemplate(TTTemplateWrapper templateW) {
		var i = TextTransformerTemplateListNode.AddItem(templateW.Value.Name);
		TextTransformerTemplateListNode.SetItemMetadata(i, templateW);
	}
	
	#endregion

	#region Text Transformer Templates
	
	private void OnTextTransformerTemplateListItemActivated(int index)
	{
		var ttTemplateW = TextTransformerTemplateListNode.GetItemMetadata(index).As<TTTemplateWrapper>();
		TTTemplateEditorNode.Load(ttTemplateW);
		TTTemplateEditorNode.Show();
	}

	private void OnTTTemplateEditorTTTemplateAdded(TTTemplateWrapper templateW)
	{
		AddCustomTemplate(templateW);
	}

	private void OnTTTemplateEditorTTTemplateUpdated(TTTemplateWrapper newTemplateW, string oldTemplateName)
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
	
	private void OnAddButtonPressed()
	{
		// TODO ugly, think of smt other
		if (TTTabContainerNode.CurrentTab == 0) {
			// text transformers
			// TODO
			return;
		}
		if (TTTabContainerNode.CurrentTab == 1) {
			// text transformer templates
			
			TTTemplateEditorNode.Load(null);
			TTTemplateEditorNode.Show();
			return;
		}
	}
	

}







