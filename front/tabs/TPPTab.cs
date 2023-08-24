using Godot;
using System;

using MtgCardParser;

// TODO? add scrolls to lists

public partial class TPPTab : TabBar
{
	#region Nodes
	public ItemList TextTransformerListNode { get; private set; }
	public ItemList TextTransformerTemplateListNode { get; private set; }
	public VBoxContainer TextBoxesNode { get; private set; }
	public TTTemplateEditor TTTemplateEditorNode { get; private set; }
	#endregion
	
	public override void _Ready() {
		#region Node fetching
		
		TextTransformerListNode = GetNode<ItemList>("%TextTransformerList");
		TextTransformerTemplateListNode = GetNode<ItemList>("%TextTransformerTemplateList");
		TextBoxesNode = GetNode<VBoxContainer>("%TextBoxes");
		TTTemplateEditorNode = GetNode<TTTemplateEditor>("%TTTemplateEditor");
		
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
		var i = TextTransformerTemplateListNode.AddItem(template.Name);
		TextTransformerTemplateListNode.SetItemMetadata(i, new TTTemplateWrapper(template));
	}
	
	#endregion

	#region Text Transformer Templates
	
	private void OnTextTransformerTemplateListItemActivated(int index)
	{
		var ttTemplateW = TextTransformerTemplateListNode.GetItemMetadata(index).As<TTTemplateWrapper>();
		TTTemplateEditorNode.Load(ttTemplateW);
		TTTemplateEditorNode.Show();
	}
	#endregion
}

public partial class TTTemplateWrapper : Node {
	public LuaTextTransformerTemplate Value { get; }
	public TTTemplateWrapper(LuaTextTransformerTemplate v) { Value = v; }
}




