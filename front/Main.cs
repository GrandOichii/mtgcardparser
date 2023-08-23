using Godot;
using System;
using System.Linq;
using System.IO;
using System.Threading;

using System.Collections.Generic;
//using Godot.Collections;
using System.Text.Json;
using System.Text.Json.Serialization;
 

public partial class Main : CanvasLayer
{
	private readonly Random Rnd = new Random();
	private readonly string BULK_DATA_INFO_URL = "https://api.scryfall.com/bulk-data";
	
	[Signal]
	public delegate void CardAddedEventHandler(SourceCard card);
	
	public List<SourceCard> Cards { get; private set; }
	
	#region Nodes
	public CardsList CardsListNode { get; private set; }
	public CardsList SampledCardsListNode { get; private set; }
	public HttpRequest BulkDataRequestNode { get; private set; }
	public ProgressBar CardsDownloadProgressBarNode { get; private set; }
	public Button DownloadCardsButtonNode { get; private set; }
	public HttpRequest CardsDownloadRequestNode { get; private set; }
	public CardViewWindow CardViewWindowNode { get; private set; }
	public SpinBox SampleSizeNode { get; private set; }
	#endregion
	
	private string _cardSrc;
	
	public override void _Ready() {
		#region Node fetching
		CardViewWindowNode = GetNode<CardViewWindow>("%CardViewWindow");
		CardsListNode = GetNode<CardsList>("%CardsList");
		SampledCardsListNode = GetNode<CardsList>("%SampledCardsList");
		BulkDataRequestNode = GetNode<HttpRequest>("%BulkDataRequest");
		CardsDownloadProgressBarNode = GetNode<ProgressBar>("%CardsDownloadProgressBar");
		DownloadCardsButtonNode = GetNode<Button>("%DownloadCardsButton");
		CardsDownloadRequestNode = GetNode<HttpRequest>("%CardsDownloadRequest");
		SampleSizeNode = GetNode<SpinBox>("%SampleSize");
		_cardSrc = CardsDownloadRequestNode.DownloadFile;
		#endregion
		
		#region Card loading
		if (File.Exists(_cardSrc)) {
			Thread t = new Thread(() => LoadCards());
			t.Start();
		}
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
		
		LoadCards();
	}
	#endregion
	
	#region Card loading
	public void LoadCards() {
		Cards = JsonSerializer.Deserialize<List<SourceCard>>(File.ReadAllText(_cardSrc));
//		SampleSizeNode.MaxValue = Cards.Count;
		foreach (var card in Cards)
			CallDeferred("emit_signal", "CardAdded", card);
	}
	#endregion


	#region Card viewing
	
	private void ViewCard(SourceCard card)
	{
		CardViewWindowNode.Load(card);
		CardViewWindowNode.Show();
	}
	
	private void OnCardViewWindowCloseRequested()
	{
		CardViewWindowNode.Hide();
	}
	
	#endregion
	
	#region Random sampling
	[Signal]
	public delegate void ClearSampledEventHandler();
	[Signal]
	public delegate void AddSampleCardEventHandler(SourceCard card);
	
	public IEnumerable<SourceCard> SampledCards { get; private set; }
	
	private void OnSampleRandomPressed()
	{
		EmitSignal(SignalName.ClearSampled);
		SampledCards = Cards.OrderBy(x => Rnd.Next()).Take((int)SampleSizeNode.Value);
		foreach (var card in SampledCards)
			EmitSignal(SignalName.AddSampleCard, card);
	}
	
	#endregion
}
