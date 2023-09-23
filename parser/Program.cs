using MtgCardParser;
using System.Xml;
using System.Xml;

using System.Text.RegularExpressions;

class Program {
    public static void Main(String[] args) {
        GenerateRandomCards();
        return;
        // var pattern = new Regex(",\\s");
        // var split = pattern.Split("{x}, {t}");
        // System.Console.WriteLine(split.Length);
        // foreach(var s in split)
        //     System.Console.WriteLine("\t" + s);
        // return;
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
        var text = "Put a +1/+1 counter on target creature with a +1/+1 counter on it.";
        var result = project.Do(new Card("Test1", text));
        foreach (var trace in result) {
            PrintTrace(trace);
            System.Console.WriteLine("---");
        }

        System.Console.WriteLine();
        System.Console.WriteLine();
        System.Console.WriteLine();

        // var unproccessedIndex = new Dictionary<PNode, List<string>>();
        // var proccessedIndex = new Dictionart<PNode, List<string>>();
        var traces = new List<ParseTrace>();
        
        foreach (var trace in result) {
            AddTraces(trace, traces);
            // var processed = new Dictionary<string, ParseTrace>();
            // var unprocessed = new Dictionary<string, ParseTrace>();

        }
        foreach (var trace in traces) {
            if (trace.Parsed)
                System.Console.WriteLine(trace.Parent.Name + " " + trace.Text);
        }
        System.Console.WriteLine();
        System.Console.WriteLine();
        foreach (var trace in traces) {
            if (!trace.Parsed)
                System.Console.WriteLine(trace.Parent.Name + " " + trace.Text);
        }
        
    }

    private static void AddTraces(ParseTrace trace, List<ParseTrace> traces) {
        if (trace is null) return;
        traces.Add(trace);
        foreach (var child in trace.ChildrenTraces)
            AddTraces(child, traces);
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
        return;
        // if (trace is null) return;
        // var name = parentName + "." + trace.Parent.Name;
        // if (IsRelevant(trace)) {
        //     result.Add(name, trace);
        // }
        // foreach (var child in trace.ChildrenTraces) {
        //     CheckForRelevantTraces(name, child, result);
        // }
    }

    private static bool IsRelevant(ParseTrace trace) {
        if (trace.Parsed) return false;
        return true;
    }

    private static Random Rnd = new();
    private static void GenerateRandomCards() {
        var testPath = "../saved-project";
        // testPath = "C:\\Users\\ihawk\\code\\mtgcardparser\\saved-project";
        System.Console.WriteLine("Loading...");
        var project = Project.Load(testPath);
        
        // var text = "{1}: Put a charge counter on Test1."; // this generates bad text
        // var text = "{T}: Draw 1 card."; // this doesn't 
        
        System.Console.WriteLine("Parcing...");
        var result = project.Do(new Card("Test1", text));
        var parcedMap = new Dictionary<PNode, List<ParseTrace>>();
        foreach (var pr in result)
            FillParcedTexts(pr, parcedMap);
        // foreach (var pair in parcedMap) {
        //     System.Console.WriteLine(pair.Key.Name);
        //     foreach (var s in pair.Value)
        //         System.Console.WriteLine("\t" + s.Text);
        // }

        System.Console.WriteLine("Generating...");
        var possibleTexts = project.GenerateAllPossibleTexts(parcedMap);
        // System.Console.WriteLine(possibleTexts[Rnd.Next() % possibleTexts.Count]);
        // return;
        System.Console.WriteLine("Saving...");
        File.WriteAllLines("../possible-texts.txt", possibleTexts);
        // foreach (var t in possibleTexts)
        //     System.Console.WriteLine(t);
    }

    private static void FillParcedTexts(ParseTrace? trace, Dictionary<PNode, List<ParseTrace>> index) {
        if (trace is null) return;
        if (!trace.Parsed) return;
        var parent = trace.Parent;
        if (!index.ContainsKey(parent)) index.Add(parent, new());
        var list = index[parent];
        
        // make all traces unique
        foreach (var t in list)
            if (t.Text == trace.Text)
                return;
        
        index[parent].Add(trace);

        foreach (var child in trace.ChildrenTraces)
            FillParcedTexts(child, index);
    }
}