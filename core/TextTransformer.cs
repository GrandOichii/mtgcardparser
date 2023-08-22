using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using NLua;

namespace MtgCardParser;

public abstract class TextTransformerTemplate {
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public List<TTArg> Args { get; set; } = new();

    abstract public StringBuilder Do(StringBuilder text, Card card, Dictionary<string, string> args);
}

public class LuaTextTransformerTemplate : TextTransformerTemplate {
    private static readonly string TRANSFORM_FNAME = "Transform";
    
    public LuaFunction TransformF { get; }
    private Lua _lState;

    public LuaTextTransformerTemplate(Lua lState, string path) {
        _lState = lState;

        var text = File.ReadAllText(path);
        var loader = TextTransformerTemplateLoader.FromJSON(text);
        
        Name = loader.Name;
        Description = loader.Description;
        Args = loader.Args;
    
        lState.DoFile(Path.Join(Directory.GetParent(path).FullName, loader.ScriptPath));
        TransformF = LuaUtility.GetGlobalF(lState, TRANSFORM_FNAME);
    }

    public override StringBuilder Do(StringBuilder text, Card card, Dictionary<string, string> args)
    {
        var returned = TransformF.Call(text.ToString(), card.ToLuaTable(_lState), LuaUtility.CreateTable(_lState, args));
        return new(LuaUtility.GetReturnAs<string>(returned));
    }
}

public class TTArg {
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    public TTArg(string name) {
        Name = name;
    }
}

class TextTransformerTemplateLoader {
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";
    [JsonPropertyName("description")]
    public string Description { get; set; } = "";
    [JsonPropertyName("args")]
    public List<TTArg> Args { get; set; } = new();
    [JsonPropertyName("script-path")]
    public string ScriptPath { get; set; } = "";

    public static TextTransformerTemplateLoader FromJSON(string json) {
        var result = JsonSerializer.Deserialize<TextTransformerTemplateLoader>(json) ?? throw new Exception("Failed to parse TextTransformerTemplateLoader JSON: " + json);
        return result;
    }
}

public class TextTransformer {
    public string Name { get; set; }
    public Dictionary<string, string> TemplateArgs { get; }
    public TextTransformerTemplate Template { get; set; }
    public TextTransformer(TextTransformerPipeline pipeline, string path) {
        var text = File.ReadAllText(path);
        var loader = TextTransformerLoader.FromJSON(text);

        Name = loader.Name;
        TemplateArgs = loader.Args;
        Template = pipeline.GetTemplate(loader.Template) ?? throw new Exception("No template with name " + loader.Template);
    }

    public string Do(string text, Card card) {
        var result = Template.Do(new(text), card, TemplateArgs);
        return result.ToString();
    }
}

class TextTransformerLoader {
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";
    [JsonPropertyName("template")]
    public string Template { get; set; } = "";
    [JsonPropertyName("args")]
    public Dictionary<string, string> Args { get; set; } = new();

    public static TextTransformerLoader FromJSON(string json) {
        var result = JsonSerializer.Deserialize<TextTransformerLoader>(json) ?? throw new Exception("Failed to parse TextTransformerLoader JSON: " + json);
        return result;
    }
}


#region Pre-built Text Transformer Loaders

public class CardNameTextTransformerTemplate : TextTransformerTemplate
{
    private readonly string PATTERN = @"\b{0}\b";
    private readonly string REPLACEMENT_ARG_NAME = "Replacement";
    public CardNameTextTransformerTemplate() {
        Name = "tt-cardname";
        Description = "TODO";

        Args.Add(new (REPLACEMENT_ARG_NAME));
    }

    public override StringBuilder Do(StringBuilder text, Card card, Dictionary<string, string> args)
    {
        return new(Regex.Replace(text.ToString(), string.Format(PATTERN, card.Name), args[REPLACEMENT_ARG_NAME]));
    }
}

public class LowerCaseTextTransformerTemplate : TextTransformerTemplate {
    public LowerCaseTextTransformerTemplate() {
        Name = "tt-lowercase";
        Description = "TODO";
    }

    public override StringBuilder Do(StringBuilder text, Card card, Dictionary<string, string> args)
    {
        return new(text.ToString().ToLower());
    }
}

public abstract class RegexSurrounderTransformerTemplate : TextTransformerTemplate {
    private readonly string SURROUND_FORMAT = "[{0}:{1}]";

    protected StringBuilder RegexDo(StringBuilder text, Card card, string pattern, string name) {
        var m = Regex.Matches(text.ToString(), pattern);
        for (int i = m.Count - 1; i >= 0; i--) {
            Match match = m[i];
            text = text.Replace(
                match.ToString(),
                string.Format(SURROUND_FORMAT, name, match.ToString()),
                match.Index,
                match.ToString().Length
            );

        } 
        return text;
    }
}

public class FormatSurrounderTextTransformerTemplate : RegexSurrounderTransformerTemplate {
    private readonly string PATTERN_ARG_NAME = "Pattern";
    private readonly string SURROUND_ARG_NAME = "Surround";
    public FormatSurrounderTextTransformerTemplate() {
        Name = "tt-formatsurrounder";
        Description = "TODO";

        Args.Add(new(PATTERN_ARG_NAME));
        Args.Add(new(SURROUND_ARG_NAME));
    }

    public override StringBuilder Do(StringBuilder text, Card card, Dictionary<string, string> args)
    {
        return RegexDo(text, card, args[PATTERN_ARG_NAME], args[SURROUND_ARG_NAME]);
    }
}

public class FileSourceSurrounderTextTransformerTemplate : RegexSurrounderTransformerTemplate {
    private readonly string SURROUND_ARG_NAME = "Surround";
    private readonly string FILE_PATH_NAME = "File path";
    private readonly string PATTERN_FORMAT = "\\b{0}\\b";
    public FileSourceSurrounderTextTransformerTemplate() {
        Name = "tt-filesourcesurrounder";
        Description = "TODO";

        Args.Add(new(FILE_PATH_NAME));
        Args.Add(new(SURROUND_ARG_NAME));
    }

    public override StringBuilder Do(StringBuilder text, Card card, Dictionary<string, string> args)
    {
        var words = File.ReadAllLines(args[FILE_PATH_NAME]);
        foreach (var word in words) {
            var pattern = string.Format(PATTERN_FORMAT, word);
            text = RegexDo(text, card, pattern, args[SURROUND_ARG_NAME]);
        }
        return text;
    }
}
#endregion