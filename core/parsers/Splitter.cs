namespace MtgCardParser;
using System.Xml;

using System.Text.RegularExpressions;

public class Splitter : PNode {
    public override string NodeName => "splitter";
    private string _patternString;
    public string PatternString { 
        get => _patternString;
        set {
            _patternString = value;
            Pattern = new Regex(value);
        }
    }
    public Regex Pattern { get; private set; }

    public static Splitter FromXml(PNodeLoader loader, XmlElement el) {
        var result = new Splitter();

        var templateA = el.Attributes.GetNamedItem("template");
        if (templateA is not null) {
            result.Name = templateA.Value;
            result.IsTemplate = true;
            return result;
        }
        result.Name = el.Attributes.GetNamedItem("name")?.Value;
        result.PatternString = el.Attributes.GetNamedItem("pattern")?.Value;
        foreach (XmlElement child in el.ChildNodes) {
            var c = loader.Load(child);
            result.Children.Add(c);
        }

        // children count should always be one
        if (result.Children.Count != 1) {
            throw new Exception("Failed to load Splitter node from XML: more than 1 child:\n" + el);
        }

        return result;
    }

    public override ParseTrace? Do(string text) {
        var split = Pattern.Split(text);
        var child = Children[0];
        var result = new ParseTrace(this, text);
        result.Parsed = true;

        foreach (var s in split) {
            var trace = child.Do(s);
            result.ChildrenTraces.Add(trace);
            if (trace is null || !trace.Parsed) result.Parsed = false;
        }

        return result;
    }

    public override XmlElement ToXml(XmlDocument doc, bool ignoreTemplate = false)
    {
        var result = base.ToXml(doc, ignoreTemplate);
        if (!ignoreTemplate && IsTemplate) return result;
        
        result.SetAttribute("pattern", PatternString);
        return result;
    }

    public override List<string>? GenerateAllPossibleTexts(Dictionary<PNode, List<ParseTrace>> index) {
        // TODO
        return Children[0].GenerateAllPossibleTexts(index);
    }

}