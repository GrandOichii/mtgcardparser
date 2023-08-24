using Godot;
using System;
using System.Threading;


public partial class CardsList : ItemList
{
	[Signal]
	public delegate void CardClickedEventHandler(SourceCard card);
	
	private void AddCard(SourceCard card)
	{
		// TODO freezes at the beginning, very slow
		var i = AddItem(card.CName);
		SetItemMetadata(i, card);
	}
	
	private void UpdateCard(SourceCard card) {
		// TODO process card changes, if card already exists
		// name should always be the same
		AddCard(card);
	}
	
	private void ClearCards() {
		Clear();
	}
	
	public SourceCard this[int i] => GetItemMetadata(i).As<SourceCard>();
	
	private void OnItemSelected(int index)
	{
		EmitSignal(SignalName.CardClicked, this[index]);
	}
}
