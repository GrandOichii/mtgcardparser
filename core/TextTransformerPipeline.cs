using System.Text.Json;
using System.Text.Json.Serialization;
using NLua;

namespace MtgCardParser;

public class TextTransformerPipeline {
    private static readonly string MANIFEST_FILE = "manifest.json";

    public List<TextTransformerTemplate> Templates { get; } = new() {
        new CardNameTextTransformerTemplate(),
        new LowerCaseTextTransformerTemplate(),
        new FormatSurrounderTextTransformerTemplate(),
        new FileSourceSurrounderTextTransformerTemplate(),
        new SimpleReplacerTextTransformerTemplate()
    };
    public List<LuaTextTransformerTemplate> CustomTemplates { get; } = new();
    public List<TextTransformer> Pipeline { get; } = new();
    public Lua LState { get; } = new();

    public static TextTransformerPipeline Load(string dir) {
        var text = File.ReadAllText(Path.Combine(dir, MANIFEST_FILE));
        var loader = TextTransformerPipelineLoader.FromJSON(text);
        var result = new TextTransformerPipeline();

        foreach (var path in loader.Templates) {
            var template = new LuaTextTransformerTemplate(result.LState, Path.Join(dir, path));
            result.CustomTemplates.Add(template);
        }
        foreach (var path in loader.Pipeline) {
            var tt = new TextTransformer(result, Path.Join(dir, path));
            result.Pipeline.Add(tt);
        }
        return result;
    }

    public TextTransformerTemplate? GetTemplate(string templateName) {
        foreach (var template in Templates)
            if (template.Name == templateName) return template;
        foreach (var template in CustomTemplates)
            if (template.Name == templateName) return template;
        return null;
    }

    public string Do(Card card) {
        var result = card.Text;

        Console.WriteLine("Initial: " + result);
        foreach (var transformer in Pipeline) {
            result = transformer.Do(result, card);
        }

        return result;
    }

    public List<string> DoDetailed(Card card) {
        var result = new List<string>();
        var text = card.Text;
        result.Add(text);
        foreach (var transformer in Pipeline) {
            text = transformer.Do(text, card);
            result.Add(text);
        }
        return result;
    }
}

class TextTransformerPipelineLoader {
    [JsonPropertyName("pipeline")]
    public List<string> Pipeline { get; set; } = new();

    [JsonPropertyName("templates")]
    public List<string> Templates { get; set; } = new();
    
    public static TextTransformerPipelineLoader FromJSON(string json) {
        var result = JsonSerializer.Deserialize<TextTransformerPipelineLoader>(json) ?? throw new Exception("Failed to parse TextTransformerPipelineLoader JSON: " + json);
        return result;
    }
}