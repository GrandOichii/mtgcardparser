using MtgCardParser;
using System.Xml;

class Program {
    public static void Main(String[] args) {
        var testPath = "../test-project";
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

        project.SaveTo("../saved-project");
    }
}

/*
Enters with an indestructable counter if you are flying with a +1/+1 counter.

[\+|\-]\d\/[\+|\-]\d counter - +x/+x counters
\b\w+\b counter - keyword counters
*/