using System.Net;
using LanguageExt;
using LanguageExt.Common;
using LanguageExt.Traits;
using static LanguageExt.Prelude;
using static LanguageExt.Http;

namespace LanguageExtHttp.Examples;

/// <summary>
/// Not intended to be run, just a place to keep the code examples from the README and make sure they still compile
/// </summary>
public static class SnippetsFromReadme
{
    public static void Run()
    {
        var snippetBindTraverse =
            from users in get("http://api-one.url/old_users").Bind(parseUsersResponse)
            from results in users.Traverse(user => post("http://api-two.url/backfill_users", serializeUser(user)))
            select results;

        var snippetFallible =
            get("http://api-one.url/old_users").Bind(parseUsersResponse)
            | @catch<Http, Seq<User>>(ParseErrorCode, HandleParseError)
            | @catch<Http, Seq<User>>(UrlErrorCode, HandleUrlError);
        
        // snippedMonadIO
        var attempts = Atom(0);
        var lookup =
            from _1 in attempts.SwapIO(i => i + 1)
            from response in get("http://api-one.url/old_users")
            from _ in IO.lift(() => Console.WriteLine($"Query attempt {attempts.Value}"))
            select response;
        lookup.RetryIO(Schedule.linear(1.Seconds()).Take(3));

        CancellationToken cancellationToken = default;
        
        var snippetRun = 
            get("http://example.com")
                .RunIO(new HttpClient()) // Run HTTP Monad
                .Run(EnvIO.New(token: cancellationToken)); 
        
        // Generalized Example Usage
        getStreamWithDebug<Http, HttpEnv>("http://example.com").As();
        
        getStreamWithDebug<Eff<MyCustomConfig>, MyCustomConfig>("http://example.com");

        getStreamWithDebug<MyCustomApp, MyCustomConfig>("http://example.com");
        
        // Mocking example
        var mockHttpClient = Http.client((HttpRequestMessage message) => new HttpResponseMessage(HttpStatusCode.OK));
    }
    // Generalized method example
    static K<M, Stream> getStreamWithDebug<M, Env>(string rawUri)
        where M : Readable<M, Env>, MonadIO<M>, Fallible<M>
        where Env : HasHttpClient
        =>
            from uri in parseUri<M>(rawUri)
            from rawResponse in get<M, Env>(uri)
            from _1 in IO.lift(() => Console.WriteLine($"Successful fetch from {rawUri}"))
            from response in stream<M>(rawResponse)
            from _2 in IO.lift(() => Console.WriteLine($"Successfully read as stream"))
            select response;


    #region DummySupportingMembersForSnippets
    public record MyCustomConfig(HttpClient Client, string ApiKey, int MagicNumber) : HasHttpClient;
    
    public record MyCustomApp<A>(ReaderT<MyCustomConfig, IO, A> run) : K<MyCustomApp, A>;

    public class MyCustomApp :
        Deriving.MonadIO<MyCustomApp, ReaderT<MyCustomConfig, IO>>,
        Deriving.Readable<MyCustomApp, MyCustomConfig, ReaderT<MyCustomConfig, IO>>,
        Fallible<MyCustomApp>
    {
        public static K<ReaderT<MyCustomConfig, IO>, A> Transform<A>(K<MyCustomApp, A> fa)
        {
            throw new NotImplementedException();
        }

        public static K<MyCustomApp, A> CoTransform<A>(K<ReaderT<MyCustomConfig, IO>, A> fa)
        {
            throw new NotImplementedException();
        }

        public static K<MyCustomApp, A> Fail<A>(Error error)
        {
            throw new NotImplementedException();
        }

        public static K<MyCustomApp, A> Catch<A>(K<MyCustomApp, A> fa, Func<Error, bool> Predicate, Func<Error, K<MyCustomApp, A>> Fail)
        {
            throw new NotImplementedException();
        }
    }

    private static Http<Seq<User>> HandleParseError(Error err) =>
        IO.lift(() => Console.WriteLine("Error parsing users, returning empty seq")) >>
        Http.Pure<Seq<User>>([]);
    
    private static Http<Seq<User>> HandleUrlError(Error err) =>
        IO.lift(() => Console.WriteLine("Error parsing Url, returning empty seq")) >>
        Http.Pure<Seq<User>>([]);
    
    private const int ParseErrorCode = 123;
    private const int UrlErrorCode = 124;

    private record User();
    private static HttpContent serializeUser(User user) => new StringContent("user");
    private static Http<Seq<User>> parseUsersResponse(HttpResponseMessage arg) => Http.Pure(Seq(new User()));

    #endregion
}