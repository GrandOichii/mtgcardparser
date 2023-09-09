using MtgCardParser;
using System.Xml;

using System.Text.RegularExpressions;

class Program {
    public static void Main(String[] args) {
        var testPath = "../saved-project";
        // testPath = "C:\\Users\\ihawk\\code\\mtgcardparser\\saved-project";
        var project = Project.Load(testPath);

        // var ttp = project.TTPipeline;
        // var card = new Card("Where Ancients Tread", "{T}: Deal 1 damage to any target.");
        // var result = ttp.Do(
        //     card
        // );
        // System.Console.WriteLine(result);
        // var root = project.Root;
        // var parsed = root.Do(result);
        // System.Console.WriteLine("Parsed: " + parsed);

        // project.SaveTo("../saved-project");
        var text = "whenever amgus, gusgus.";
        var result = project.Do(new Card("Test1", text));
        foreach (var trace in result) {
            PrintTrace(trace);
            System.Console.WriteLine("---");
        }

        System.Console.WriteLine();
        System.Console.WriteLine();
        System.Console.WriteLine();

        var unproccessedIndex = new Dictionary<PNode, List<string>>();

        foreach (var trace in result) {
            var relevant = new Dictionary<string, ParseTrace>();
            CheckForRelevantTraces("", trace, relevant);
            foreach (var pair in relevant) {
                System.Console.WriteLine(pair.Key + ": " + pair.Value.Text);
                var pNode = pair.Value.Parent;
                if (!unproccessedIndex.ContainsKey(pNode)) unproccessedIndex.Add(pNode, new());
                unproccessedIndex[pNode].Add(pair.Value.Text);
            }
        }

        System.Console.WriteLine();
        System.Console.WriteLine();
        System.Console.WriteLine();

        foreach (var pair in unproccessedIndex) {
            System.Console.WriteLine(pair.Key.Name);
            foreach (var t in pair.Value)
                System.Console.WriteLine("\t" + t);
        }
        
    }

    private readonly static int INDENT_STEP = 4;
    private static void PrintTrace(ParseTrace? trace, int indent=0) {
        var indented = new String(' ', indent);
        if (trace is null) {
            System.Console.WriteLine(indented + "null");
            return;
        }

        System.Console.WriteLine(indented + string.Format("({0}: {1})", trace.Parent.Name, trace.Text));
        System.Console.WriteLine(indented + "Parsed: " + trace.Parsed);
        foreach (var child in trace.ChildrenTraces)
            PrintTrace(child, indent+INDENT_STEP);
    }

    private static void CheckForRelevantTraces(string parentName, ParseTrace? trace, Dictionary<string, ParseTrace> result) {
        if (trace is null) return;
        var name = parentName + "." + trace.Parent.Name;
        if (IsRelevant(trace)) {
            result.Add(name, trace);
        }
        foreach (var child in trace.ChildrenTraces) {
            CheckForRelevantTraces(name, child, result);
        }
    }

    private static bool IsRelevant(ParseTrace trace) {
        if (trace.Parsed) return false;
        return true;
    }


}

/*
Enters with an indestructable counter if you are flying with a +1/+1 counter.

[\+|\-]\d\/[\+|\-]\d counter - +x/+x counters
\b\w+\b counter - keyword counters
*/