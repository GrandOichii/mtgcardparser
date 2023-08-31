using System.Text.Json;
using System.Text.Json.Serialization;

namespace MtgCardParser;

public class Project {
    private static readonly string MANIFEST_FILE = "manifest.json";

    public TextTransformerPipeline TTPipeline { get; }
    public List<PNode> Parsers { get;}

    /// <summary>
    /// Loads the project from the specified directory
    /// </summary>
    /// <param name="dir">The directory of the project</param>
    public static Project Load(string dir) {
        var mText = File.ReadAllText(Path.Combine(dir, MANIFEST_FILE));
        var pLoader = ProjectLoader.FromJson(mText);
        
        // load text transormer pipeline
        var ttPipeline = TextTransformerPipeline.Load(Path.Join(dir, pLoader.TTPDir));

        // load parsers
        var pl = new PNodeLoader();
        var parsers = pl.LoadNodesFromDir(Path.Combine(dir, pLoader.ParsersDir));

        return new(ttPipeline, parsers);
    }

    public Project(TextTransformerPipeline ttPipeline, List<PNode> parsers) {
        TTPipeline = ttPipeline;
        Parsers = parsers;
    }

    private static readonly string TPP_DIR = "ttp";
    public void SaveTo(string dir) {
        // delete directory if already exists
        if (Directory.Exists(dir)) {
            Directory.Delete(dir, true);
        }

        // create directory
        Directory.CreateDirectory(dir);

        // add manifest file
        var pLoader = new ProjectLoader();
        pLoader.TTPDir = TPP_DIR;
        // TODO parsers

        // save manifest file
        var pLoaderJ = JsonSerializer.Serialize(pLoader);
        File.WriteAllText(Path.Combine(dir, MANIFEST_FILE), pLoaderJ);

        var ttpDir = Path.Combine(dir, TPP_DIR);

        // create ttp directory
        Directory.CreateDirectory(ttpDir);

        TTPipeline.SaveTo(ttpDir);
    }

    public PNode Root {
        get {
            foreach (var node in Parsers)
                if (node.Name == "root")
                    return node;
            throw new Exception("No root parser present");
        }
    }
}

class ProjectLoader {
    [JsonPropertyName("ttp-dir")]
    public string TTPDir { get; set; } = "";
    [JsonPropertyName("parsers")]
    public string ParsersDir { get; set; }

    public static ProjectLoader FromJson(string json) {
        var result = JsonSerializer.Deserialize<ProjectLoader>(json) ?? throw new Exception("Failed to deserialize ProjectLoader from JSON: " + json);
        return result;
    }
}