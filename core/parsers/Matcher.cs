namespace MtgCardParser;
using System.Xml;

using System.Text.RegularExpressions;

public class Matcher : PNode {
    public override string NodeName => "matcher";
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

        result.CheckGroupCount();

        return result;
    }

    public void CheckGroupCount() {
        if (GroupCount != Children.Count) {
            throw new Exception("Inconsistent Matcher: number of groups=" + GroupCount + ", children count=" + Children.Count);
        }
    }

    public override string ToString() {
        var result = "";

        result += Name + " " + Pattern + "[";
        foreach (var child in Children)
            result += "\n" + child.ToString();

        return result + "\n]";
    }

    public override bool Do(string text) {
        CheckGroupCount();

        var match = Pattern.Match(text);
        if (!match.Success) return false;
        
        for (int i = 1; i < match.Groups.Count; i++) {
            var child = Children[i-1];
            var group = match.Groups[i];
            var success = child.Do(group.ToString());
            if (!success) return false;
        }

        // Match m = matches[i];
        // for (int i = 0; i < matches.Count; i++) {
        //     Match match = matches[i];
        //     System.Console.WriteLine("\t" + i + " " + match);
        // }
        System.Console.WriteLine(Name);
        return true;
    }

    public override XmlElement ToXml(XmlDocument doc, bool ignoreTemplate = false)
    {
        var result = base.ToXml(doc, ignoreTemplate);
        if (!ignoreTemplate && IsTemplate) return result;
        
        result.SetAttribute("pattern", PatternString);
        return result;
    }
}