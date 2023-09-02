using System.Text.Json;
using System.Text.Json.Serialization;

namespace MtgCardParser;

public class ParseTrace {
    public PNode Parent { get; }
    public string Text { get; }
    public bool Parsed { get; set; } = false;
    public List<ParseTrace?> ChildrenTraces { get; } = new();
    public ParseTrace(PNode parent, String text) { Parent = parent; Text = text; }
}


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
    private static readonly string PARSERS_DIR = "parsers";
    public void SaveTo(string dir) {
        // delete directory if already exists
        if (Directory.Exists(dir)) {
            Directory.Delete(dir, true);
        }

        // create directory
        Directory.CreateDirectory(dir);

        // add manifest file
        var pLoader = new ProjectLoader
        {
            TTPDir = TPP_DIR,
            ParsersDir = PARSERS_DIR
        };

        // save manifest file
        var pLoaderJ = JsonSerializer.Serialize(pLoader);
        File.WriteAllText(Path.Combine(dir, MANIFEST_FILE), pLoaderJ);

        var ttpDir = Path.Combine(dir, pLoader.TTPDir);
        var parsersDir = Path.Combine(dir, pLoader.ParsersDir);

        // create ttp directory
        Directory.CreateDirectory(ttpDir);

        // create parsers directory
        Directory.CreateDirectory(parsersDir);

        TTPipeline.SaveTo(ttpDir);
        PNodeLoader.SaveParsers(Parsers, parsersDir);
    }

    public PNode Root {
        get {
            foreach (var node in Parsers)
                if (node.Name == "root")
                    return node;
            throw new Exception("No root parser present");
        }
    }

    public List<ParseTrace?> Do(Card card) {
        var result = new List<ParseTrace?>();
        var text = TTPipeline.Do(card);
        var lines = text.Split(Environment.NewLine);
        foreach (var line in lines) {
            result.Add(Root.Do(line));
        }

        return result;
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