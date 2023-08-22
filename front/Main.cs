using Godot;
using System;
using System.IO;

using System.Collections.Generic;
//using Godot.Collections;
using System.Text.Json;
using System.Text.Json.Serialization;
 

public partial class Main : CanvasLayer
{
	private readonly string BULK_DATA_INFO_URL = "https://api.scryfall.com/bulk-data";
	
	[Signal]
	public delegate void CardAddedEventHandler(SourceCard card);
	
	#region Nodes
	public VBoxContainer CardsListNode { get; private set; }
	public HttpRequest BulkDataRequestNode { get; private set; }
	public ProgressBar CardsDownloadProgressBarNode { get; private set; }
	public Button DownloadCardsButtonNode { get; private set; }
	public HttpRequest CardsDownloadRequestNode { get; private set; }
	#endregion
	
	public override void _Ready() {
		#region Node fetching
		CardsListNode = GetNode<VBoxContainer>("%CardsList");
		BulkDataRequestNode = GetNode<HttpRequest>("%BulkDataRequest");
		CardsDownloadProgressBarNode = GetNode<ProgressBar>("%CardsDownloadProgressBar");
		DownloadCardsButtonNode = GetNode<Button>("%DownloadCardsButton");
		CardsDownloadRequestNode = GetNode<HttpRequest>("%CardsDownloadRequest");
		#endregion
	}
	
	public override void _Process(double delta) {
		if (Downloading) {
			CardsDownloadProgressBarNode.Value = CardsDownloadRequestNode.GetDownloadedBytes() * 8;
		}
	}
	
	#region Cards downloading
	private bool _downloading = false;
	public bool Downloading {
		get => _downloading;
		set {
			_downloading = value;
			CardsDownloadProgressBarNode.Visible = value;
			DownloadCardsButtonNode.Disabled = value;
		}
	}
	
	class BulkDataJson {
		[JsonPropertyName("data")]
		public List<BulkDataEntryJson> Data { get; set; } = new();

		public static BulkDataJson FromJSON(string json) {
			var result = JsonSerializer.Deserialize<BulkDataJson>(json) ?? throw new Exception("Failed to deserialize BulkDataJson: " + json);
			return result;
		}

		public BulkDataEntryJson? this[string name] {
			get {
				foreach (var entry in Data)
					if (entry.Type == name) return entry;
				return null;
			}
		}
	}

	class BulkDataEntryJson {
		[JsonPropertyName("type")]
		public string Type { get; set; } = "";
		[JsonPropertyName("download_uri")]
		public string DownloadURI { get; set; } = "";
		[JsonPropertyName("size")]
		public int Size { get; set; }
	}

	private void OnDownloadCardsButtonPressed()
	{
		Downloading = true;
		BulkDataRequestNode.Request(BULK_DATA_INFO_URL);
	}
	
	private void OnBulkDataRequestRequestCompleted(long result, long response_code, string[] headers, byte[] body)
	{
		// TODO check response code
		var text = System.Text.Encoding.Default.GetString(body);
		var data = BulkDataJson.FromJSON(text);
		var oracleCards = data["oracle_cards"];
		if (oracleCards is null) throw new Exception("Failed to find the oracle cards download link");

		CardsDownloadProgressBarNode.MaxValue = oracleCards.Size;
		var downloadURI = oracleCards?.DownloadURI;
		CardsDownloadRequestNode.Request(downloadURI);
	}
	
	private void OnCardsDownloadRequestRequestCompleted(long result, long response_code, string[] headers, byte[] body)
	{
		// TODO after re-downloading cards within the same session progress bar shows status as 99%, then jumps to zero
		Downloading = false;
		
		var path = CardsDownloadRequestNode.DownloadFile;
		var cards = JsonSerializer.Deserialize<List<SourceCard>>(File.ReadAllText(path));
//		foreach ()
		GD.Print(cards.Count);
	}
	#endregion
}






