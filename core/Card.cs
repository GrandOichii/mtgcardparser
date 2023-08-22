using NLua;

namespace MtgCardParser;

public class Card {
    public string Name { get; set; }
    public string Text { get; set; }

    public Card(string name, string text) {
        Name = name;
        Text = text;
    }

    public LuaTable ToLuaTable(Lua lState) {
        var result = LuaUtility.CreateTable(lState);
        result["Name"] = Name;
        result["Text"] = Text;
        return result;
    }
}