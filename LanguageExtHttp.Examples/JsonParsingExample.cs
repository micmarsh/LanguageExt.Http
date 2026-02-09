using System.Text.Json;

namespace LanguageExtHttp.Examples;
using LanguageExt;
using static LanguageExt.Http;
using static LanguageExt.Prelude;

public static class JsonParsingExample
{
    public record Product(int id, string title);
    
    public static void Run()
    {
        var printTitle = 
            from product in (get("https://dummyjson.com/products/1") >> stream >> deserialize<Product>)
            from _1 in IO.lift(() => Console.WriteLine(product))
            select unit;

        printTitle.Run().Run();
    }
}