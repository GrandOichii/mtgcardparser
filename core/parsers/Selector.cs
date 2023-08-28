namespace MtgCardParser;
using System.Xml;

public class Selector : PNode {

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

    public override string ToString() {
        var result = Name + " [";
        foreach (var child in Children)
            result += "\n" + child.ToString();
        return result + "\n]";
    }

    public override bool Do(string text) {
        if (Children.Count == 0) {
            System.Console.WriteLine(Name);
            return true;
        }

        foreach (var child in Children) {
            var success = child.Do(text);
            if (success) {
                System.Console.WriteLine(Name);
                return true;
            } 
        }
        return false;
    }
}