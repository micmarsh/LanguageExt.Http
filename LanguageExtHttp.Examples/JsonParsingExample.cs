namespace LanguageExtHttp.Examples;
using LanguageExt;
using static LanguageExt.Http;
using static LanguageExt.Prelude;
using static LanguageExt.Json<LanguageExt.Http>;

public static class JsonParsingExample
{
    public record Product(int id, string title, string description);

    private static IO<Unit> log(object? obj) => IO.lift(() => Console.WriteLine(obj));
    
    public static void Run()
    {
        // Demo of usage of "stream" and "deserialize" with .NET 10 operators
        var printFirstProduct =
            get("https://dummyjson.com/products/1") >> stream >> (deserialize<Product>) >> log;

        printFirstProduct.RunIO().Run();

        // Detail more intricate parsing of nested json structures
        var allProducts = get("https://dummyjson.com/products") >>
                             stream >> parse >>
                             key("products");
        var printAllTitles =
            from products in allProducts >> iterate
            from titles in products.Traverse(key("title") >> cast<string>)
            from _1 in titles.Traverse(log)
            select unit;

        printAllTitles.RunIO().Run();

        // Not very efficient in terms of netowrk requests, but showing off monadic reuse and sending Json requests
        var fourthProduct = 
            allProducts >> index(3) >> cast<Product>;

        var updateFourthDescription =
            from product in fourthProduct
            let updated = product with { description = "The best product" }
            from response in patch($"https://dummyjson.com/products/{product.id}", content(updated.Json())) 
                             >> ensureSuccessStatus
                             >> @string
            from _0 in log($"Original: {product}")
            from _1 in log($"Updated: {response}")
            select unit;

        updateFourthDescription.RunIO().Run();
    }
    
    
}