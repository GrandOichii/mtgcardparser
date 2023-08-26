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
    
    public LuaFunction TransformF { get; private set; }
    private string _script  = "";
    public string Script { 
        get => _script;
        set {
            _script = value;
            LState.DoString(value);
            TransformF = LuaUtility.GetGlobalF(LState, TRANSFORM_FNAME);
        }
    }

    public Lua LState { get; set; }

    public LuaTextTransformerTemplate() {}

    public LuaTextTransformerTemplate(Lua lState, string path) {
        LState = lState;

        var text = File.ReadAllText(path);
        var loader = TextTransformerTemplateLoader.FromJSON(text);
        
        Name = loader.Name;
        Description = loader.Description;
        Args = loader.Args;

        var fPath = Path.Join(Directory.GetParent(path).FullName, loader.ScriptPath);
        // lState.DoFile(fPath);
        // TransformF = LuaUtility.GetGlobalF(lState, TRANSFORM_FNAME);
        Script = File.ReadAllText(fPath);
    }

    public override StringBuilder Do(StringBuilder text, Card card, Dictionary<string, string> args)
    {
        var returned = TransformF.Call(text.ToString(), card.ToLuaTable(LState), LuaUtility.CreateTable(LState, args));
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
    public string Name { get; set; } = "";
    public Dictionary<string, string> TemplateArgs { get; } = new();
    public TextTransformerTemplate Template { get; set; }
    public TextTransformer() {}
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
        Description = "Replaces all instances of the card name with the specified replacement string";

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
        Description = "Converts all characters to lowercase.";
    }

    public override StringBuilder Do(StringBuilder text, Card card, Dictionary<string, string> args)
    {
        return new(text.ToString().ToLower());
    }
}

public class SimpleReplacerTextTransformerTemplate : TextTransformerTemplate {
    private readonly string REGEX_PATTERN_FORMAT = "{0}";
    private readonly string REPLACED_PHRASE_ARG_NAME = "Pattern";
    private readonly string REPLACEMENT_PHRASE_ARG_NAME = "Replacement";
    public SimpleReplacerTextTransformerTemplate() {
        Name = "tt-simple-replacer";
        Description = "Replaces all matches of the pattern with another string";

        Args.Add(new(REPLACED_PHRASE_ARG_NAME));
        Args.Add(new(REPLACEMENT_PHRASE_ARG_NAME));
    }

    public override StringBuilder Do(StringBuilder text, Card card, Dictionary<string, string> args)
    {
        var pattern = string.Format(REGEX_PATTERN_FORMAT, args[REPLACED_PHRASE_ARG_NAME]);
        var replacement = args[REPLACEMENT_PHRASE_ARG_NAME];
        var m = Regex.Matches(text.ToString(), pattern);
        for (int i = m.Count - 1; i >= 0; i--) {
            Match match = m[i];
            text = text.Replace(
                match.ToString(),
                replacement,
                match.Index,
                match.ToString().Length
            );

        }
        return text;
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
        Description = "Surrounds the words that match the pattern with the specified surround word.";

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
        Description = "Surround all the words that are included in the specified file with the specified replacement string.";

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