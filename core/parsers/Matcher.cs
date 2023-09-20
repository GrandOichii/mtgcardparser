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
            Pattern = new Regex("^" + value + "$", RegexOptions.Multiline);
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

    // public override string ToString() {
    //     var result = "";

    //     result += Name + " " + Pattern + "[";
    //     foreach (var child in Children)
    //         result += "\n" + child.ToString();

    //     return result + "\n]";
    // }

    public override ParseTrace? Do(string text) {
        CheckGroupCount();

        var match = Pattern.Match(text);
        // if (Name == "simple-numeric") {
        //     System.Console.WriteLine("AM");
        // }
        if (!match.Success) return null;
        var result = new ParseTrace(this, text);
        result.Parsed = true;
        for (int i = 1; i < match.Groups.Count; i++) {
            var child = Children[i-1];
            var group = match.Groups[i];
            var trace = child.Do(group.ToString());
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
        if (Name == "TODO") return null;
        if (Name == "aa") return null;
        if (Name == "trigger") return null;
        var result = new List<string>();
        // if no children
        // if 

        if (Children.Count == 0) {
            result.Add(PatternString);
            if (!index.ContainsKey(this)) {
                return result;
            }
            result = new();
            foreach (var s in index[this])
                result.Add(s.Text);
            return result;
        }
        

        var collections = new List<List<string>>();
        foreach (var child in Children) {
            var sub = child.GenerateAllPossibleTexts(index);
            if (sub is null) return null;
            collections.Add(sub);
        }
        var combinations = Utility.GetAllPossibleCombos(collections);
        // var combinations = Utility.GetLightCombos(collections);
        // TODO could be a problem with texts containing \\(\\)
        var p = new Regex("\\(.+?\\)");
        foreach (var arr in combinations) {
            var text = PatternString;
            foreach (var s in arr) {
                text = p.Replace(text, s, 1);
            }
            result.Add(text);
        }
        return result;
    }

}