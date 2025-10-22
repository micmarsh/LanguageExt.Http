// See https://aka.ms/new-console-template for more information

using LanguageExt;
using LanguageExt.Net;
using LanguageExt.Traits;
using static LanguageExt.Net.Http;

var withInt = Readable.ask<MyApp, int>();

var got =
    from response in get<MyApp>("https://example.com")
    from client in Readable.ask<MyApp, HttpClient>()
    from number in Readable.ask<MyApp, int>()
    from yo in IO.liftAsync(env => client.GetAsync($"http://example.com/{number}", env.Token))
    select yo.EnsureSuccessStatusCode();

public record Config(int MagicNumber, HttpEnv Http, string MagicText);

public record MyApp<A>(ReaderT<Config, IO, A> run) : K<MyApp, A>;

public class MyApp: Deriving.MonadUnliftIO<MyApp, ReaderT<Config, IO>>,
    Deriving.Readable<MyApp, Config, ReaderT<Config, IO>>,
    CanRead<MyApp, Config, HttpEnv>,
    CanRead<MyApp, Config, HttpClient>,
    CanRead<MyApp, Config, int>
{
    public static K<ReaderT<Config, IO>, A> Transform<A>(K<MyApp, A> fa)
    {
        throw new NotImplementedException();
    }

    public static K<MyApp, A> CoTransform<A>(K<ReaderT<Config, IO>, A> fa)
    {
        throw new NotImplementedException();
    }


    public static HttpEnv GetInner(Config env)
    {
        return env.Http;
    }

    static int CanRead<MyApp, Config, int>.GetInner(Config env)
    {
        return env.MagicNumber;
    }

    static HttpClient CanRead<MyApp, Config, HttpClient>.GetInner(Config env)
    {
        return env.Http.Client;
    }
}

public interface CanRead<Self, OuterEnv, InnerEnv> : Readable<Self, InnerEnv>
    where Self : 
    //Functor<Self>,
    CanRead<Self, OuterEnv, InnerEnv>,
    Readable<Self, OuterEnv>
{
    public static abstract InnerEnv GetInner(OuterEnv env);
    public static abstract OuterEnv SetInner(OuterEnv outer, InnerEnv inner);

    static K<Self, InnerEnv> Readable<Self, InnerEnv>.Ask =>
        //Readable.ask<Self, OuterEnv>().Map(Self.GetInner);
        Self.Asks((InnerEnv x) => x);

    static K<Self, A> Readable<Self, InnerEnv>.Asks<A>(Func<InnerEnv, A> f) =>
        Readable.asks<Self, OuterEnv, A>(outer => f(Self.GetInner(outer)));

    // todo need to test for this, not quite sure what's going on?
    static K<Self, A> Readable<Self, InnerEnv>.Local<A>(Func<InnerEnv, InnerEnv> f, K<Self, A> ma) =>
        Self.Local(outer => Self.SetInner(outer, f(Self.GetInner(outer))), ma);
}

