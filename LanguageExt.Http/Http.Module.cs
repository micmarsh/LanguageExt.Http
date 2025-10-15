using LanguageExt.Traits;
using static LanguageExt.Prelude;

namespace LanguageExt;

public partial class Http
{
    public static K<M, HttpEnv> ask<M>() where M : Readable<M, HttpEnv> => Readable.ask<M, HttpEnv>();
    
    public static K<M, HttpResponseMessage> get<M>(Uri url, Option<HttpCompletionOption> option = default)
        where M : Readable<M, HttpEnv>, MonadIO<M>
        => ask<M>()
            .Bind(httpEnv =>
                IO.liftAsync(env => httpEnv
                    .Client
                    .GetAsync(url,
                        option.IfNone(httpEnv.CompletionOption.IfNone(HttpCompletionOption.ResponseContentRead)),
                        httpEnv.Token.IfNone(env.Token))));

    // todo cancellation token argument for these? May be a bit much to keep track of for priority?
    public static Http<HttpResponseMessage> get(Uri url, Option<HttpCompletionOption> option = default) =>
        get<Http>(url, option).As();

    public static K<M, string> readContentAsString<M>(HttpResponseMessage message)
        where M : Readable<M, HttpEnv>, MonadIO<M>
        => ask<M>().Bind(httpEnv => liftIO(env =>
            message.Content.ReadAsStringAsync(httpEnv.Token.IfNone(env.Token)))
        );
    
    public static Http<string> readContentAsString(HttpResponseMessage message) => 
        readContentAsString<Http>(message).As();
        
    public static K<M, Stream> readContentAsStream<M>(HttpResponseMessage message)
        where M : Readable<M, HttpEnv>, MonadIO<M>
        => ask<M>().Bind(httpEnv => liftIO(env =>
            message.Content.ReadAsStreamAsync(httpEnv.Token.IfNone(env.Token)))
        );
    
    public static Http<Stream> readContentAsStream(HttpResponseMessage message) => 
        readContentAsStream<Http>(message).As();

    public static K<F, Uri> parseUri<F>(string url) where F : Fallible<F>, Applicative<F>
        => Try.lift(() => new Uri(url)).Match(F.Pure, F.Fail<Uri>);

    public static Http<Uri> parseUri(string url) => parseUri<Http>(url).As();

    public static K<F, HttpResponseMessage> ensureSuccessStatus<F>(HttpResponseMessage r)
        where F : Fallible<F>, Applicative<F>
        => Try.lift(() =>
            {
                r.EnsureSuccessStatusCode();
                return r;
            })
            .Match(Applicative.pure<F, HttpResponseMessage>,
                Fallible.error<F, HttpResponseMessage>);
    
    public static Http<HttpResponseMessage> ensureSuccessStatus(HttpResponseMessage r) =>
        ensureSuccessStatus<Http>(r).As();

    public static HttpClient client(Func<HttpRequestMessage, CancellationToken, HttpResponseMessage> sendAsync) =>
        new (new MessageHandler((m, t) => Task.FromResult(sendAsync(m, t))));
    
    public static HttpClient client(Func<HttpRequestMessage, HttpResponseMessage> sendAsync) =>
        new (new MessageHandler((m, t) => Task.FromResult(sendAsync(m))));
    
    public static HttpClient client(Func<HttpRequestMessage, IO<HttpResponseMessage>> sendAsync) =>
        new (new MessageHandler((m, t) => 
            sendAsync(m).RunAsync(EnvIO.New(token: t))
                .AsTask()));
    
    public static HttpClient client(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> sendAsync) =>
        new (new MessageHandler(sendAsync));
    
    private class MessageHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> Run) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Run(request, cancellationToken);
    }
}