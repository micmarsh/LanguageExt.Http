using System.Text.Json;

namespace LanguageExtHttp.Examples;
using LanguageExt;
using static LanguageExt.Http;
using static LanguageExt.Prelude;

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

        // Technically a more details demo than above, but primarily of the dire need for nicer json
        // parsing functions (key lookup, enumeration, etc.). Coming soon, God willing.
        var printAllTitles = get("https://dummyjson.com/products") >> stream >> parse >>
                             (json => @try(() => json.GetProperty("products"))) >>
                             (json => @try(json.EnumerateArray)) >>
                             (arr => toSeq(arr).Kind().Traverse(deserialize<Product>)
                                 .MapT(p => p.title))
                             >> log;

        printAllTitles.Run().Run();
    }
}