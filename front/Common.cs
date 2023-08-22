using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;


public partial class SourceCard : Node
{
	[JsonPropertyName("name")]
	public string Name { get; set; }
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
}
