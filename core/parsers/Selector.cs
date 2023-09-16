namespace MtgCardParser;
using System.Xml;

public class Selector : PNode {
    public override string NodeName => "selector";
    public static Selector FromXml(PNodeLoader loader, XmlElement el) {
        var result = new Selector();

        var templateA = el.Attributes.GetNamedItem("template");
        if (templateA is not null) {
            result.Name = templateA.Value;
            result.IsTemplate = true;
            return result;
        }

        result.Name = el.Attributes.GetNamedItem("name")?.Value;
        foreach (XmlElement child in el.ChildNodes) {
            var c = loader.Load(child);
            result.Children.Add(c);
        }

        return result;
    }

    // public override string ToString() {
    //     var result = Name + " [";
    //     foreach (var child in Children)
    //         result += "\n" + child.ToString();
    //     return result + "\n]";
    // }

    public override ParseTrace? Do(string text) {
        var result = new ParseTrace(this, text);
        bool allNull = true;
        bool restIsNull = false;
        foreach (var child in Children) {
            // if (restIsNull) {
            //     result.ChildrenTraces.Add(null);
            //     continue;
            // }
            var cTrace = child.Do(text);
            result.ChildrenTraces.Add(cTrace);
            if (cTrace is null) continue;

            // restIsNull = true;
            allNull = false;
            if (cTrace.Parsed) {
                result.Parsed = true;
            }
            break;
        }
        // if (allNull) return null;

        return result;
    }
}