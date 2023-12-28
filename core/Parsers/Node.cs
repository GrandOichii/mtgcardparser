namespace MtgCardParser.Parsers;

using System.Xml;
using System.Text.Json;
using System.Text.Json.Serialization;


public class ParseTrace {
    public PNode Parent { get; }
    public string Text { get; }
    public bool Parsed { get; set; } = false;
    public List<ParseTrace?> ChildrenTraces { get; } = new();
    public ParseTrace(PNode parent, String text) { Parent = parent; Text = text; }
}


public abstract class PNode {
    virtual public string NodeName => "no-node-name";
    public string Name { get; set; } = "";
    public bool IsTemplate { get; set; } = false;
    public List<PNode> Children { get; set; } = new();
    abstract public ParseTrace? Do(string text);
    public void SaveTo(string path) {
        var doc = new XmlDocument();

        doc.AppendChild(ToXml(doc, true));

        doc.Save(path);
    }

    public virtual XmlElement ToXml(XmlDocument doc, bool ignoreTemplate=false) {
        var result = doc.CreateElement(NodeName);
        
        if (!ignoreTemplate && IsTemplate) {
            result.SetAttribute("template", Name);
            return result;
        }
        result.SetAttribute("name", Name);

        foreach (var child in Children)
            result.AppendChild(child.ToXml(doc));
        return result;
    }

    public virtual List<string>? GenerateAllPossibleTexts(Dictionary<PNode, List<ParseTrace>> index) {
        return null;
    }
}


public class PNodeLoader {
    static readonly string MANIFEST_FILE = "manifest.json";
    static readonly string PARSER_SAVE_EXTENSION = ".xml";

    private static readonly Dictionary<string, Func<PNodeLoader, XmlElement, PNode>> NODE_XML_PARSING_MAP = new() {
        { "matcher", Matcher.FromXml },
        { "selector", Selector.FromXml },
        { "splitter", Splitter.FromXml },
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

    static private void ReplaceTemplates(Dictionary<string, PNode> tIndex, PNode node) {
        for (int i = 0; i < node.Children.Count; i++) {
            var child = node.Children[i];
            if (!child.IsTemplate) {
                ReplaceTemplates(tIndex, child);
                continue;
            }

            node.Children[i] = tIndex[child.Name];
        }
    }

    private PNode LoadTemplate(string path) {
        var text = File.ReadAllText(path);
        XmlDocument xDoc = new();
        xDoc.LoadXml(text);

        return LoadTemplate(xDoc.DocumentElement ?? throw new Exception("No root element in document " + path));
    }

    static public void SaveParsers(List<PNode> parsers, string dir) {
        // create manifest file
        var loader = new PNodePathsLoader();

        foreach (var parser in parsers) {
            // save parser
            var fName = parser.Name + PARSER_SAVE_EXTENSION;
            var file = Path.Combine(dir, fName);
            parser.SaveTo(file);

            if (parser.Name == "root") {
                loader.RootPath = fName;
                continue;
            }
            loader.TemplatePaths.Add(fName);
        }

        // save the manifest file
        var loaderJ = JsonSerializer.Serialize(loader);
        File.WriteAllText(Path.Combine(dir, MANIFEST_FILE), loaderJ);
    }
}


public class PNodePathsLoader {
    [JsonPropertyName("templates")]
    public List<string> TemplatePaths { get; set; } = new();
    [JsonPropertyName("root")]
    public string RootPath { get; set; } = "";

    public static PNodePathsLoader FromJson(string text) {
        return JsonSerializer.Deserialize<PNodePathsLoader>(text) ?? throw new Exception("Failed to deserialize json to PNodePathsLoader: " + text);
    }
}