using System.Net;
using System.Net.Http.Json;

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

        // Not very efficient in terms of network requests, but showing off monadic reuse and sending Json requests
        var fourthProduct = 
            allProducts >> index(3) >> cast<Product>;

        Http<Product> sendUpdateRequest(Product updated) =>
            patch($"https://dummyjson.com/products/{updated.id}", content(updated.Json())) 
            >> ensureSuccessStatus
            >> stream >> (deserialize<Product>);

        var updateFourthDescription =
            from product in fourthProduct
            let updated = product with { description = "The best product" }
            from response in sendUpdateRequest(updated)
            from _0 in log($"Original: {product}")
            from _1 in log($"Updated: {response}")
            select unit;

        updateFourthDescription.RunIO(EchoJsonClient).Run();
    }

    /// <summary>
    /// For some reason https://dummyjson.com/docs/products#products-update doesn't work as expected (or I'm missing
    /// something, more likely), so this provides a quick and dirty way for the last example to run without breaking
    /// </summary>
    public static readonly HttpClient EchoJsonClient = client(request => request.Content switch
    {
        JsonContent jsonContent => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = jsonContent
        },
        _ => (new HttpClient()).Send(new HttpRequestMessage(request.Method, request.RequestUri))
    });
}
