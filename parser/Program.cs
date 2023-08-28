using MtgCardParser;
using System.Xml;

class Program {
    public static void Main(String[] args) {
        var testPath = "../test-project";
        var project = Project.Load(testPath);

        // var ttp = project.TTPipeline;
        // var result = ttp.Do(
        //     new Card("Where Ancients Tread", "Whenever a creature with power 5 or greater enters the battlefield under your control, you may have Where Ancients Tread deal 5 damage to target creature or player.")
        // );

        // var pl = new PNodeLoader();
        // var text = File.ReadAllText("test.xml");
        // XmlDocument xDoc = new XmlDocument();
        // xDoc.LoadXml(text);

        // var e = pl.Load(xDoc.DocumentElement);
        // System.Console.WriteLine(e.ToString());
    }
}

/*
Enters with an indestructable counter if you are flying with a +1/+1 counter.

[\+|\-]\d\/[\+|\-]\d counter - +x/+x counters
\b\w+\b counter - keyword counters
*/