using Godot;
using System;

namespace FrontV2;

public partial class CardsTab : Control
{
	#region Nodes
	
	public HttpRequest DownloadRequestNode { get; private set; }
	public ProgressBar DownloadProgressNode { get; private set; }
	
	#endregion

	public override void _Ready()
	{
		#region Node fetching
		
		DownloadRequestNode = GetNode<HttpRequest>("%DownloadRequest");
		DownloadProgressNode = GetNode<ProgressBar>("%DownloadProgress");
		
		#endregion
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
