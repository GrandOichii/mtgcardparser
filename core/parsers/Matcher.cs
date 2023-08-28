namespace MtgCardParser;
using System.Xml;

using System.Text.RegularExpressions;

public class Matcher : PNode {
    private string _patternString;
    public string PatternString { 
        get => _patternString;
        set {
            _patternString = value;
            Pattern = new Regex(value);
        }
    }
    public Regex Pattern { get; private set; }

    // each one corresponds to it's respective capture group in the pattern
    public List<PNode> Children { get; } = new();

    public int GroupCount => Pattern.GetGroupNames().Length - 1;

    public static Matcher FromXml(PNodeLoader loader, XmlElement el) {
        var result = new Matcher();

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

        if (GroupCount != result.Children.Count) {
            throw new Exception("Inconsistent Matcher: number of groups=" + GroupCount + ", children count=" + result.Children.Count);
        }

        return result;
    }

    public override string ToString() {
        var result = "";

        result += Name + " " + Pattern + "[";
        foreach (var child in Children)
            result += "\n" + child.ToString();

        return result + "\n]";
    }
}