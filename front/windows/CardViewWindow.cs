using Godot;
using System;

public partial class CardViewWindow : Window
{
	#region Nodes
	public Label NameLabelNode { get; private set; }
	public Label CostLabelNode { get; private set; }
	public Label PowerLabelNode { get; private set; }
	public Label ToughnessLabelNode { get; private set; }
	public Label TextLabelNode { get; private set; }
	public TextureRect ImageNode { get; private set; }
	public HttpRequest ImageRequestNode { get; private set; }
	#endregion
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		#region Node fetching
		TextLabelNode = GetNode<Label>("%TextLabel");
		ToughnessLabelNode = GetNode<Label>("%ToughnessLabel");
		PowerLabelNode = GetNode<Label>("%PowerLabel");
		CostLabelNode = GetNode<Label>("%CostLabel");
		NameLabelNode = GetNode<Label>("%NameLabel");
		ImageNode = GetNode<TextureRect>("%Image");
		ImageRequestNode = GetNode<HttpRequest>("%ImageRequest");
		#endregion
	}

	public void Load(SourceCard card) {
		Title = card.CName;
		
		NameLabelNode.Text = card.CName;
		CostLabelNode.Text = card.ManaCost;
		PowerLabelNode.Text = card.Power;
		ToughnessLabelNode.Text = card.Toughness;
		TextLabelNode.Text = card.Text;
		
		// load image
		ImageRequestNode.Request(card.ImageURIs["normal"]);
	}
	
	private void OnImageRequestRequestCompleted(long result, long response_code, string[] headers, byte[] body)
	{
		// TODO fit window size to UI
		var image = new Image();
		image.LoadJpgFromBuffer(body);	
		var tex = ImageTexture.CreateFromImage(image);
		ImageNode.Texture = tex;
	}
}


