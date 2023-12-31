using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

using MtgCardParser;


public partial class Wrapper<T> : Node
{
	public T Value { get; }
	public Wrapper(T v) { Value = v; }
}

public partial class SourceCard : Node
{
	[JsonPropertyName("name")]
	public string CName { get; set; }
	[JsonPropertyName("image_uris")]
	public Dictionary<string, string> ImageURIs { get; set; }
	[JsonPropertyName("mana_cost")]
	public string ManaCost { get; set; }
	[JsonPropertyName("oracle_text")]
	public string Text { get; set; }
	[JsonPropertyName("colors")]
	public List<string> Colors { get; set; }
	[JsonPropertyName("typeline")]
	public string Typeline { get; set; }
	[JsonPropertyName("power")]
	public string Power { get; set; }
	[JsonPropertyName("toughness")]
	public string Toughness { get; set; }

	public Card ToCard() => new Card(CName, Text);
}

public partial class PNodeWrapper : Wrapper<PNode>
{
	public PNodeWrapper(PNode v) : base(v) { }

	public int GetChildIndex(PNode child)
	{
		for (int i = 0; i < Value.Children.Count; i++)
		{
			if (Value.Children[i] == child)
				return i;
		}
		return -1;
	}
}

public interface PNodeEditor
{
	public bool IsVisible();
	public void SetVisible(bool v);
	public void ClearValues() { }
	public PNode GetBakedPNode() => null;
}

public static class GUtil
{
	public static void Alert(Node parent, string message)
	{
		// TODO use own dialog box
		OS.Alert(message, "MTG Card Parser");
	}

	public static bool CheckNameTaken(string newName, string oldName, List<string> names) {
		if (newName == oldName) return true;
		foreach (var name in names) {
			if (name != newName) continue;

			return false;
		}
		return true;
	}
}
