namespace MtgCardParser;
using System.Xml;

public class Selector : PNode {
    public List<PNode> Children { get; } = new();

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
            result.Children.Add(cMatcher);
        }

        return result;
    }

    public override string ToString() {
        var result = Name + " [";
        foreach (var child in Children)
            result += "\n" + child.ToString();
        return result + "\n]";
    }
}