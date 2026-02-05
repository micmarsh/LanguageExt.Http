# LanguageExt HTTP

### WIP I'm currently in the process of cleaning this up for a non-alpha release, some of this documentation may not be accurate

A functional wrapper around HttpClient intended to integrate into LanguageExt V5 based workflows.

Provides the expected methods (`get`, `post`, `delete`, etc.) returning `Http<HttpResponseMessage>`, an "Http Monad"
## Rationale
If you're already convinced of the general preferability of the functional approach, you probably don't need this `Rationale` section.

If you're not convinced but curious, check out the [code cleanup project](https://github.com/micmarsh/LanguageExt.Http/tree/master/LanguageExt.Net.Http.CodeCleanupExample/Program.cs), noting the differences between the functional and imperative approaches, not just in total lines of code, but also the greater simplicity[^1] of nearly every part of the functional approach.

I may create a more dedicated "literate coding" style writeup of the above in the future (as of 10/23/2025), but for now hopefully the code can speak for iself.

[^1] Simplicity in the [Rich Hickey sense of the word](https://www.youtube.com/watch?v=SxdOUGdseq4), it may not be _easy_ at first if you're not familiar with the concepts!

## Usage
Add `LanguageExt.Net.Http 0.1.0-alpha-4` on nuget.

```csharp
// add to GlobalUsings as appropriate
using LanguageExt;// <- you're probbably already using this
using static LanguageExt.Http;
```
The `Http` monad implements and thus gives us
* `Monad`, for basic sequencing and composition, `Bind` (with LINQ syntax) and `Traverse` being the bread and butter of most of what you'll do
```csharp
// `parseUsersResponse` made up for example purposes
from users in get("http://api-one.url/old_users").Bind(parseUsersResponse) 
// `serializeUser` made up as an example
from results in users.Traverse(user => post("http://api-two.url/backfill_users", serializeUser(user)))
select results
```
* `Fallible`, for generalizable and modular error handling
```csharp
get("http://api-one.url/old_users").Bind(parseUsersResponse) 
    // error codes and handlers made up for example purposes
    | @catch<Http, Seq<Users>>(ParseErrorCode, HandleParseError)
    | @catch<Http, Seq<Users>>(UrlErrorcode, HandleUrlError)
```
* `MonadUnliftIO` for not only lifting arbitrary IO operations (such as debugging logs), but also access to `Retry`, `Fork`, `Repeat` and related goodes
```csharp
var attempts = Atom(0);
var lookup =
    from _1 in attempts.SwapIO(i => i + 1)
    from response in get("http://api-one.url/old_users")
    from _ in IO.lift(() => Console.WriteLine($"Query attempt {attempts.Value}"))
    select response;
lookup.RetryIO(Schedule.linear(1.Seconds()).Take(3));
```
* `Readable`, to enable threading of a`HttpClient` throughout the application
  * If you need to thread `CancellationToken` as well, you can utilize `IO`'s built-in `EnvIO`
```csharp
get("http://example.com")
    .Run(new HttpClient()) // Run HTTP Monad
    .Run(EnvIO.New(token: cancellationToken)); // Normal IO Monad run
```
### Usage in Larger Applications 

However, since a concrete `Http` type is an obstacle to composition in large applications, nearly every method in this library has both an `Http`-based and generalized version, for exmaple
* "The basics" (`get`, `post`, `delete`, etc.), can be generalized to any `MonadIO` that implements `Readable` for an `Env` that implements this library's `HasHttpClient` interface
* `parseUri` can be generalized to any `Fallible` `Applicative`
* Response parsing methods such as `readContentAsStream` can be generalized to any `MonadIO`

For example, if we have the following hypothetical method
```csharp
K<M, Stream> getStreamWithDebug<M, Env>(string rawUri)
    where M : Readable<M, Env>, MonadIO<M>, Fallible<M>
    where Env : HasHttpClient
    =>
        from uri in parseUri<M>(rawUri)
        from rawResponse in get<M, Env>(uri)
        from _1 in IO.lift(() => Console.WriteLine($"Successful fetch from {rawUri}"))
        from response in readContentAsStream<M>(rawResponse)
        from _2 in IO.lift(() => Console.WriteLine($"Successfully read as stream"))
        select response;
```
We can use it with this libarary's `Http`
```csharp
// genericMethod<>().As() is how most of this library 
// is currently implemented under the hood
getStreamWithDebug<Http, HttpEnv>("http://example.com").As();
````
With LanguageExt's built-in`Eff`
```csharp
public record MyCustomConfig(HttpClient Client, string ApiKey, int MagicNumber) : HasHttpClient;
getStreamWithDebug<Eff<MyCustomConfig>, MyCustomConfig>("http://example.com");
```
Or with your application's very own monad(s)
```csharp
public record MyCustomApp<A>(ReaderT<MyCustomConfig, IO, A> run) : K<MyCustomApp, A>;
// ... full implemetation of above omitted for brevity ...

getStreamWithDebug<MyCustomApp, MyCustomConfig>("http://example.com");
```
### Testing
Mocking `HttpClient` [is much more awkward than it should be](https://stackoverflow.com/questions/36425008/mocking-httpclient-in-unit-tests), so this library provides a `Http.client` method that, given a `Func<HttpResponseMessage, HttpResponseMessage>` ( [or other overload](https://github.com/micmarsh/LanguageExt.Http/blob/master/LanguageExt.Net.Http/Module/Http.Module.Client.cs) ) handles all of the nasty business of dealing with an `HttpMessageHandler` for you.
```csharp
var mockHttpClient = Http.client((HttpResponseMessage message) => new HttpResponseMessage(HttpStatusCode.OK));
```
This combined with the natural structure of the "reader monad pattern" this follows should enable much smoother mocking of http functionality in general. It may even be convenient enough to justify sneaking this library (and by extension LanguageExt) into a "regular" imperative/OO codebase that uses `HttpClient`!

## TODO
There's a lot of work to be done on "the LanguageExt ecosystem" in general, as V5 itself is technically still in beta. 
Feel free to open discussions, issues or PRs to communicate how this library can better fit your particular use case

Copyright 2025 Michael Marsh
