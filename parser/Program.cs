using MtgCardParser;

class Program {
    public static void Main(String[] args) {
        var testPath = "../saved-project";
        var project = Project.Load(testPath);

        var ttp = project.TTPipeline;
        var result = ttp.Do(
            new Card("Where Ancients Tread", "Whenever a creature with power 5 or greater enters the battlefield under your control, you may have Where Ancients Tread deal 5 damage to target creature or player.")
        );
    }
}