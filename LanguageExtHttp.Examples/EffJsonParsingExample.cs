using System.Net;
using System.Net.Http.Json;

namespace LanguageExtHttp.Examples;
using LanguageExt;
using static LanguageExt.Http;
using static LanguageExt.Prelude;
using static LanguageExt.Json<LanguageExt.Eff<ExampleEnv>>;

public static class EffJsonParsingExample
{
    public record Meta(DateTime updatedAt);
    public record Product(int id, string title, string description, Meta meta);

    private static IO<Unit> log(object? obj) => IO.lift(() => Console.WriteLine(obj));
    private static IO<Unit> sessionLog(ExampleEnv env, object? obj) => 
        env.Session >> (id => log($"Session {id}: {obj}"));
    
    private static ExampleEnv Env => new ExampleEnv(new HttpClient(),
        IO.lift(() => "UserSession123"),
        IO.lift(() => DateTime.Now));
    
    public static void Run()
    {
        // Demo of usage of "stream" and "deserialize" with .NET 10 operators
        var printFirstProduct =
            get<Eff<ExampleEnv>, ExampleEnv>("https://dummyjson.com/products/1")
            >> (stream<Eff<ExampleEnv>>) >> (deserialize<Product>) >> log;

        printFirstProduct.RunUnsafe(Env);

        // Detail more intricate parsing of nested json structures
        var allProducts = get<Eff<ExampleEnv>, ExampleEnv>("https://dummyjson.com/products") >>
                             (stream<Eff<ExampleEnv>>) >> parse >>
                             key("products");
        var printAllTitles =
            from products in allProducts >> iterate
            from titles in products.Traverse(key("title") >> cast<string>)
            from env in ask<ExampleEnv>()
            from _0 in sessionLog(env, "found products:")
            from _1 in +titles.Traverse(log) //todo PR in main lib to smooth confusing compile error and need for `+`
            select unit;

        printAllTitles.RunUnsafe(Env);

        // Not very efficient in terms of network requests, but showing off monadic reuse and sending Json requests
        var fourthProduct = 
            allProducts >> index(3) >> cast<Product>;

        Eff<ExampleEnv, Product> sendUpdateRequest(Product updated) =>
            patch<Eff<ExampleEnv>, ExampleEnv>($"https://dummyjson.com/products/{updated.id}", content(updated.Json())) 
            >> (ensureSuccessStatus<Eff<ExampleEnv>>)
            >> (stream<Eff<ExampleEnv>>) >> (deserialize<Product>);

        var updateFourthDescription =
            from product in fourthProduct
            from env in ask<ExampleEnv>()
            from now in env.Now
            let updated = product with { description = "The best product", meta = new (updatedAt: now)}
            from response in sendUpdateRequest(updated)
            from _0 in sessionLog(env, $"Original: {product}")
            from _1 in sessionLog(env, $"Updated: {response}")
            select unit;

        updateFourthDescription.RunUnsafe(Env with { Client = EchoJsonClient });
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
