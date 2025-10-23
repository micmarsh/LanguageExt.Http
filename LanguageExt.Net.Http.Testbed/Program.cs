// See https://aka.ms/new-console-template for more information

using LanguageExt;
using LanguageExt.Net;
using LanguageExt.Traits;
using static LanguageExt.Net.Http;
using Http = LanguageExt.Http;

var got =
    from response in get<MyApp, Config>("https://example.com")
    from config in Readable.ask<MyApp, Config>()
    let number = config.MagicNumber
    from yo in IO.liftAsync(env => config.Client.GetAsync($"http://example.com/{number}", env.Token))
    select yo.EnsureSuccessStatusCode();

public record Config(int MagicNumber, HttpEnv Http, string MagicText)
    : HasHttpClient
{
    public HttpClient Client => Http.Client; 
}

public record MyApp<A>(ReaderT<Config, IO, A> run) : K<MyApp, A>;

public class MyApp: Deriving.MonadUnliftIO<MyApp, ReaderT<Config, IO>>,
    Deriving.Readable<MyApp, Config, ReaderT<Config, IO>>
{
    public static K<ReaderT<Config, IO>, A> Transform<A>(K<MyApp, A> fa)
    {
        throw new NotImplementedException();
    }

    public static K<MyApp, A> CoTransform<A>(K<ReaderT<Config, IO>, A> fa)
    {
        throw new NotImplementedException();
    }
}

