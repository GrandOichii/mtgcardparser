using Godot;
using System;

public partial class TTTemplateEditor : Window
{
	#region Nodes
	public TextEdit DescriptionEditNode { get; private set; }
	public ItemList ArgumentListNode { get; private set; }
	public LineEdit NewArgumentEditNode { get; private set; }
	public CodeEdit ScriptEditNode { get; private set; }
	#endregion
	
	public override void _Ready()
	{
		#region Node fetching
		
		DescriptionEditNode = GetNode<TextEdit>("%DescriptionEdit");
		ArgumentListNode = GetNode<ItemList>("%ArgumentList");
		NewArgumentEditNode = GetNode<LineEdit>("%NewArgumentEdit");
		ScriptEditNode = GetNode<CodeEdit>("%ScriptEdit");

		#endregion
	}

	public void Load(TTTemplateWrapper ttTemplateW) {
		
	}
}
