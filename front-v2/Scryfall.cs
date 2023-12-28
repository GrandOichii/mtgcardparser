using Godot;
using System;
using System.Text.Json.Serialization;

namespace FrontV2;

public class DataContainer<T> {
	[JsonPropertyName("data")]
	public T Data { get; set; }
}
