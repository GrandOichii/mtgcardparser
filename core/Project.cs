using System.Text.Json;
using System.Text.Json.Serialization;

namespace MtgCardParser;

public class Project {
    private static readonly string MANIFEST_FILE = "manifest.json";

    public TextTransformerPipeline TTPipeline { get; }

    /// <summary>
    /// Loads the project from the specified directory
    /// </summary>
    /// <param name="dir">The directory of the project</param>
    public static Project Load(string dir) {
        var mText = File.ReadAllText(Path.Combine(dir, MANIFEST_FILE));
        var pLoader = ProjectLoader.FromJson(mText);
        
        // load text transormer pipeline
        var ttPipeline = TextTransformerPipeline.Load(Path.Join(dir, pLoader.TTPDir));

        return new(ttPipeline);
    }

    private Project(TextTransformerPipeline ttPipeline) {
        TTPipeline = ttPipeline;
    }
}

class ProjectLoader {
    [JsonPropertyName("ttp-dir")]
    public string TTPDir { get; set; } = "";

    public static ProjectLoader FromJson(string json) {
        var result = JsonSerializer.Deserialize<ProjectLoader>(json) ?? throw new Exception("Failed to deserialize ProjectLoader from JSON: " + json);
        return result;
    }
}