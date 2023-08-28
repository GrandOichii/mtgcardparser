namespace MtgCardParser;

using System.Xml;
using System.Text.Json;
using System.Text.Json.Serialization;


public abstract class PNode {
    public string Name { get; set; }
    public bool IsTemplate { get; set; } = false;
    public List<PNode> Children { get; } = new();
    abstract public bool Do(string text);
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
        var root = LoadTemplate(Path.Combine(dir, loader.RootPath));
        result.Add(root);
        foreach (var path in loader.TemplatePaths) {
            var template = LoadTemplate(Path.Combine(dir, path));
            result.Add(template);
        }

        // create template name index
        var tIndex = new Dictionary<string, PNode>();
        foreach (var node in result) {
            tIndex.Add(node.Name, node);
        }

        foreach (var template in result) {
            ReplaceTemplates(tIndex, template);
        }

        // iterate over all, replace templates
        return result;
    }

    private void ReplaceTemplates(Dictionary<string, PNode> tIndex, PNode node) {
        for (int i = 0; i < node.Children.Count; i++) {
            var child = node.Children[i];
            if (!child.IsTemplate) continue;

            node.Children[i] = tIndex[child.Name];
        }
    }

    private PNode LoadTemplate(string path) {
        var text = File.ReadAllText(path);
        XmlDocument xDoc = new XmlDocument();
        xDoc.LoadXml(text);

        return Load(xDoc.DocumentElement);
    }
}

public class PNodePathsLoader {
    [JsonPropertyName("templates")]
    public List<string> TemplatePaths { get; set; }
    [JsonPropertyName("root")]
    public string RootPath { get; set; }

    public static PNodePathsLoader FromJson(string text) {
        return JsonSerializer.Deserialize<PNodePathsLoader>(text) ?? throw new Exception("Failed to deserialize json to PNodePathsLoader: " + text);
    }
}