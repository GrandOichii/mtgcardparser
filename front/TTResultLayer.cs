using Godot;
using System;

public partial class TTResultLayer : VBoxContainer
{
	#region Nodes
	
	public Label TTNameNode { get; private set; }
	public RichTextLabel TTTextNode { get; private set; }
	
	#endregion
	
	public override void _Ready()
	{
		#region Node fetching
		
		TTNameNode = GetNode<Label>("%TTName");
		TTTextNode = GetNode<RichTextLabel>("%TTText");
		
		#endregion
	}

	public void Load(string name, string text) {
		TTNameNode.Text = name;
		TTTextNode.Text = text;
		
		GD.Print(text);
	}
}
