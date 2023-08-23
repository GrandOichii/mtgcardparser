using Godot;
using System;
using System.Threading;


public partial class CardsList : VBoxContainer
{
	static readonly PackedScene SelectableCardListItemPS = ResourceLoader.Load("uid://bsqjop3gdbynk") as PackedScene;

	public override void _Ready()
	{
	}

	public override void _Process(double delta)
	{
	}

	private void OnMainCardAdded(SourceCard card)
	{
		// TODO process card changes, if card already exists
		// TODO freezes at the beginning, very slow
		var cardNode = SelectableCardListItemPS.Instantiate() as SelectableCardListItem;
		
		AddChild(cardNode);
		cardNode.Load(card);
	}
}
