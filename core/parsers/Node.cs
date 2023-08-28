namespace MtgCardParser;
using System.Xml;

public abstract class PNode {
    public string Name { get; set; }
    public bool IsTemplate { get; set; } = false;
}


public class PNodeLoader {
    static readonly string MANIFEST_FILE = "manifest.json";
    // TODO after loading all templates, iterate over all of the pnodes and replace them if they are marked as IsTemplate nad have the same name as a template in the collection
    private static readonly Dictionary<string, Func<PNodeLoader, XmlElement, PNode>> NODE_XML_PARSING_MAP = new() {
        { "matcher", Matcher.FromXml },
        { "selector", Selector.FromXml },
    };

    public PNode Load(XmlElement el) {
        var name = el.Name;
        if (!NODE_XML_PARSING_MAP.ContainsKey(name)) {
            throw new Exception("Cannot parse pnode name " + name);
        }

        var parser = NODE_XML_PARSING_MAP[name];
        return parser(this, el);
    }

    public PNode LoadTemplate(XmlElement el) {
        var result = Load(el);
        result.IsTemplate = true;
        return result;
    }

    public List<PNode> LoadNodesFromDir(string dir) {
        var loaderT = File.ReadAllText(Path.Combine(dir, MANIFEST_FILE));
        var loader = PNodePathsLoader.FromJson(loaderT);
        
        // load raw pnodes
        var result = new List<PNode>();
        result.Add(root);
        var root = LoadTemplate(Path.Combine(dir, loader.RootPath));
        foreach (var path in loader.TemplatePaths) {
            var template = LoadTemplate(Path.Combine(dir, path));
            result.Add(template);
        }

        // iterate over all, replace templates
        return result;
    }

    private PNode LoadTemplate(string path) {
        var text = File.ReadAllText(path);
        XmlDocument xDoc = new XmlDocument();
        xDoc.LoadXml(text);

        return Load(path);
}

public class PNodePathsLoader {
    [JsonPropertyName("templates")]
    public List<string> TemplatePaths { get; set; }
    [JsonPropertyName("root")]
    public string RootPath { get; set; }

    public static PNodePathsLoader FromJson(string text) {
        return JsonSerializer.Deserialize<PNodePathsLoader>(text) ?? throw new Exception("Failed to deserialize json to PNodePathsLoader: " + json);
    }
}