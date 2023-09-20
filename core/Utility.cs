namespace MtgCardParser;
using NLua;

static class LuaUtility {
    /// <summary>
    /// Fetches global function from Lua state
    /// </summary>
    /// <param name="lState">Lua state</param>
    /// <param name="fName">Function name</param>
    /// <returns>The function</returns>
    static public LuaFunction GetGlobalF(Lua lState, string fName) {
        var f = lState[fName] as LuaFunction ?? throw new Exception("Failed to get function " + fName + " from glabal Lua state");
        return f;
    }

    /// <summary>
    /// Checks whether can index the array, if not throws exception
    /// </summary>
    /// <param name="returned">The array</param>
    /// <param name="index">The index</param>
    static void CheckIndex(object[] returned, int index) {
        if (index < returned.Length) return;

        throw new Exception("Can't access return value with index " + index + ": total amount of returned values is " + returned.Length);
    }

    /// <summary>
    /// Selects the return value at specified index and returns it as an object of the specified type
    /// </summary>
    /// <param name="returned">Array of the returned values</param>
    /// <param name="index">Index of the return value</param>
    /// <typeparam name="T">Type of the return value</typeparam>
    static public T GetReturnAs<T>(object[] returned, int index=0) where T : class {
        CheckIndex(returned, index);
        var result = returned[index] as T ?? throw new Exception("Return value in index " + index + " is not a table");
        return result;
    }

    /// <summary>
    /// Creates a new Lua table
    /// </summary>
    /// <param name="lState">Lua state</param>
    /// <returns>New table</returns>
    static public LuaTable CreateTable(Lua lState) {
        lState.NewTable("_table");
        return lState.GetTable("_table");
    }

    /// <summary>
    /// Creates a new Lua array
    /// </summary>
    /// <param name="lState">Lua state</param>
    /// <param name="args">The array</param>
    /// <returns>Lua array</returns>
    static public LuaTable CreateTable<T>(Lua lState, List<T> args) {
        var result = CreateTable(lState);
        for (int i = 0; i < args.Count; i++)
            result[i+1] = args[i];
        return result;
    }

    // /// <summary>
    // /// Creates a new Lua table
    // /// </summary>
    // /// <param name="lState">Lua state</param>
    // /// <param name="args">The dictionary used as a template</param>
    // /// <returns>Lua table</returns>    
    // static public LuaTable CreateTable(Lua lState, Dictionary<string, object> args) {
    //     var result = CreateTable(lState);
    //     foreach (var pair in args)
    //         result[pair.Key] = pair.Value;
    //     return result;
    // }

    /// <summary>
    /// Creates a new Lua table
    /// </summary>
    /// <param name="lState">Lua state</param>
    /// <param name="args">The dictionary used as a template</param>
    /// <returns>Lua table</returns>    
    static public LuaTable CreateTable<T>(Lua lState, Dictionary<string, T> args) {
        var result = CreateTable(lState);
        foreach (var pair in args)
            result[pair.Key] = pair.Value;
        return result;
    }


}

public static class Utility {

    // static public List<string> Combinations(List<List<string>> collections) {
    //     var result = new List<string>();
    //     BuildPossibleCombination(collections, 0, result);
    //     return result;
    // }

    // private static void BuildPossibleCombination(List<List<string>> collections, int level, List < string > output) {
    //     if (level < collections.Count) {
    //         foreach(string value in collections[level]) {
    //             List < string > resultList = new List < string > ();
    //             resultList.AddRange(output);
    //             resultList.Add(value);
    //             BuildPossibleCombination(collections, level + 1, resultList);
    //         }
    //     }
    // }
    public static IEnumerable<TSource> Append<TSource>(
        this IEnumerable<TSource> source, TSource item)
    {
        foreach (TSource element in source)
            yield return element;

        yield return item;
    }

    public static IEnumerable<IEnumerable<string>> GetAllPossibleCombos(
        IEnumerable<IEnumerable<string>> strings)
    {
        IEnumerable<IEnumerable<string>> combos = new string[][] { new string[0] };

        foreach (var inner in strings)
            combos = from c in combos
                    from i in inner
                    select c.Append(i);

        return combos;
    }

    // public static IEnumerable<IEnumerable<string>> GetLightCombos(IEnumerable<IEnumerable<string>> strings) {

    // }


}