using Godot;
using System;
using System.Collections.Generic;

using MtgCardParser;

public partial class TTEditor : Window
{
	public List<string> TTNames { get; set; }
	private bool _editMode;
	private Wrapper<TextTransformer> _last;
	
	public override void _Ready()
	{
	}
	
	public void Load(Wrapper<TextTransformer>? ttW) {
		TextTransformer tt;
		_editMode = ttW is not null;
		
		if (_editMode) {
			_last = ttW;
			tt = ttW?.Value;
		} else {
			tt = new TextTransformer();
		}
	}
	
	#region Text transformer templates
	
	private void OnTTTemplateEditorTTTemplateAdded(Wrapper<LuaTextTransformerTemplate> templateW)
	{
		TemplateOptionNode.AddItem(templateW.Value.Name);
	}
	
	
	#endregion
}
