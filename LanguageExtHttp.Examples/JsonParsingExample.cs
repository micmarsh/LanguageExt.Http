namespace LanguageExtHttp.Examples;
using LanguageExt;
using static LanguageExt.Http;
using static LanguageExt.Prelude;
using static LanguageExt.Json;

public static class JsonParsingExample
{
    public record Product(int id, string title);

    private static IO<Unit> log(object? obj) => IO.lift(() => Console.WriteLine(obj));
    
    public static void Run()
    {
        // Demo of usage of "stream" and "deserialize" with .NET 10 operators
        var printFirstProduct =
            get("https://dummyjson.com/products/1") >> stream >> (deserialize<Product>) >> log;

        printFirstProduct.Run().Run();

        // Detail more intricate parsing of nested json structures
        var printAllTitles = get("https://dummyjson.com/products") >> stream >> parse >>
                             key<Http>("products") >>
                             (iterate<Http>) >>
                             (elts => elts.Traverse(key<Http>("title") >> cast<string>))
                             >> (elts => elts.Traverse(log));

        printAllTitles.Run().Run();
    }
}