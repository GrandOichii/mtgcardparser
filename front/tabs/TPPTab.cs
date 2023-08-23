using Godot;
using System;

public partial class TPPTab : TabBar
{
	#region Project loading
	
	private void OnProjectLoaded(ProjectWrapper project)
	{
		var p = project.Project;
		GD.Print(p);
	}
	
	#endregion
}


