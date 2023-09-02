using Godot;
using System;
using System.Linq;
using System.IO;
using System.Threading;

using System.Collections.Generic;
//using Godot.Collections;
using System.Text.Json;
using System.Text.Json.Serialization;

using MtgCardParser;
 
// TODO:
// Make card list better - add ability to manually add cards to sampled list + removing them

public partial class Main : CanvasLayer
{
	private readonly Random Rnd = new Random();
	private readonly string BULK_DATA_INFO_URL = "https://api.scryfall.com/bulk-data";
	
	[Signal]
	public delegate void CardAddedEventHandler(SourceCard card);
	
	public List<SourceCard> Cards { get; private set; }
	
	#region Nodes
	
	public Control MainControlNode { get; private set; }
	public CardsList CardsListNode { get; private set; }
	public CardsList SampledCardsListNode { get; private set; }
	public HttpRequest BulkDataRequestNode { get; private set; }
	public ProgressBar CardsDownloadProgressBarNode { get; private set; }
	public Button DownloadCardsButtonNode { get; private set; }
	public HttpRequest CardsDownloadRequestNode { get; private set; }
	public CardViewWindow CardViewWindowNode { get; private set; }
	public SpinBox SampleSizeNode { get; private set; }
	public TTPTab TTPNode { get; private set; }
	public ParsersTab ParsersNode { get; private set; }
	
	#endregion
	
	private string _cardSrc;

	public string ProjectPath { get; set; } = "";
	
	public override void _Ready() {
		#region Node fetching
		
		MainControlNode = GetNode<Control>("%MainControl");
		CardViewWindowNode = GetNode<CardViewWindow>("%CardViewWindow");
		CardsListNode = GetNode<CardsList>("%CardsList");
		SampledCardsListNode = GetNode<CardsList>("%SampledCardsList");
		BulkDataRequestNode = GetNode<HttpRequest>("%BulkDataRequest");
		CardsDownloadProgressBarNode = GetNode<ProgressBar>("%CardsDownloadProgressBar");
		DownloadCardsButtonNode = GetNode<Button>("%DownloadCardsButton");
		CardsDownloadRequestNode = GetNode<HttpRequest>("%CardsDownloadRequest");
		SampleSizeNode = GetNode<SpinBox>("%SampleSize");
		TTPNode = GetNode<TTPTab>("%TTP");
		ParsersNode = GetNode<ParsersTab>("%Parsers");
		
		_cardSrc = CardsDownloadRequestNode.DownloadFile;
		#endregion
		
		#region Card loading
		
		if (File.Exists(_cardSrc)) {
			// TODO move back to threads
//			Thread t = new Thread(() => LoadCards());
//			t.Start();
			LoadCards();
		}
		
		#endregion
		
		// TODO remove
		GetNode<TabContainer>("MainControl/TabContainer").CurrentTab = 2;
		SampleSizeNode.Value = 100;
		OnSampleRandomPressed();
		Load("../saved-project");
	}
	
	public override void _Process(double delta) {
		if (Downloading) {
			CardsDownloadProgressBarNode.Value = CardsDownloadRequestNode.GetDownloadedBytes() * 8;
		}
	}
	
	private readonly static int SIZE_MOD = 5;
	public override void _Input(InputEvent e) {
		if (e.IsActionPressed("save"))
			SaveAction();
		if (e.IsActionPressed("inc-size"))
			ModFont(SIZE_MOD);
		if (e.IsActionPressed("dec-size"))
			ModFont(-SIZE_MOD);
	}
	
	#region Font size
	
	private void ModFont(int v) {
		// TODO don't think this should be done this way
		MainControlNode.Theme.DefaultFontSize += v;
	}
	
	#endregion
	
	#region Actions
	
	private void SaveAction() {
		// GD.Print(ProjectPath.Length + "---");
		if (ProjectPath.Length == 0) {
			// TODO
			return;
		}
		
		var project = BakedProject;
		project.SaveTo(ProjectPath);
		GD.Print("Saved!");
	}
	
	#endregion
	
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
	
	public List<SourceCard> SampledCards { get; private set; }
	
	private void OnSampleRandomPressed()
	{
		EmitSignal(SignalName.ClearSampled);
		// SampledCards = Cards.OrderBy(x => Rnd.Next()).Take((int)SampleSizeNode.Value).ToList();
		SampledCards = Cards;
		foreach (var card in SampledCards)
			EmitSignal(SignalName.AddSampleCard, card);
	}
	
	#endregion
	
	#region Project loading
	
	[Signal]
	public delegate void ProjectLoadedEventHandler(Wrapper<Project> projectW);
	
	public void Load(string projectPath) {
		var project = Project.Load(projectPath);
		EmitSignal(SignalName.ProjectLoaded, new Wrapper<Project>(project));

		ProjectPath = projectPath;
	}
	
	#endregion
	
	public Project BakedProject {
		get {
			var ttp = TTPNode.BakedPipeline;
			var parsers = ParsersNode.BakedParsers;
			var result = new Project(ttp, parsers);

			
			return result;
		}
	}
	
	private void OnBakeButtonPressed()
	{
		var project = BakedProject;
		var unparcedTextIndex = new Dictionary<PNode, List<string>>();
		
		foreach (var card in SampledCards) {
			var traces = project.Do(card.ToCard());
			foreach (var trace in traces) {
				if (trace.Parsed) continue;
				
				FillUnparcedTextIndex(trace, unparcedTextIndex);
			}
			
		}
		
		foreach (var pair in unparcedTextIndex) {
			GD.Print(pair.Key.Name);
			foreach (var t in pair.Value)
				GD.Print("\t" + t);
			
		}
		
//		ParsersNode.UnparcedTextIndex = unparcedTextIndex;
	}
	
	private bool TraceMatches(ParseTrace trase) {
		return !trase.Parsed;
	}
	
	private void FillUnparcedTextIndex(ParseTrace? trace, Dictionary<PNode, List<string>> index) {
		if (trace is null) return;
		if (TraceMatches(trace)){
			if (!index.ContainsKey(trace.Parent)) index.Add(trace.Parent, new());
			index[trace.Parent].Add(trace.Text);
		}
		foreach (var child in trace.ChildrenTraces)
			FillUnparcedTextIndex(child, index);
	}
}


