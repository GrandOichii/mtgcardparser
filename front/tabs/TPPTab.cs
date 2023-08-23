using Godot;
using System;

public partial class TPPTab : TabBar
{
	#region Nodes
	public ItemList TextTransformerListNode { get; private set; }
	public ItemList TextTransformerTemplateListNode { get; private set; }
	public VBoxContainer TextBoxesNode { get; private set; }
	#endregion
	
	public override void _Ready() {
		TextTransformerListNode = GetNode<ItemList>("%TextTransformerList");
		TextTransformerTemplateListNode = GetNode<ItemList>("%TextTransformerTemplateList");
		TextBoxesNode = GetNode<VBoxContainer>("%TextBoxes");
	}
	
	#region Project loading
	
	private void OnProjectLoaded(ProjectWrapper project)
	{
		var p = project.Project;
		
	}
	
	#endregion
}
