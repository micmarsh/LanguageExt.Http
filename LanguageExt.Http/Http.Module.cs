using LanguageExt.Traits;
using static LanguageExt.Prelude;

namespace LanguageExt;

public partial class Http
{
    
    public static Http<HttpResponseMessage> get(Uri url, Option<HttpCompletionOption> option = default) => new (
        new ReaderT<HttpEnv, IO, HttpResponseMessage>(
            httpEnv =>
                IO.liftAsync(
                    env => httpEnv
                        .Client
                        .GetAsync(url,
                            option.IfNone(httpEnv.CompletionOption.IfNone(HttpCompletionOption.ResponseContentRead)), 
                            httpEnv.Token.IfNone(env.Token)))
        )
    );

    public static Http<string> readContentAsString(HttpResponseMessage message) =>
        new (
            new ReaderT<HttpEnv, IO, string>(httpEnv => liftIO(env => 
                message.Content.ReadAsStringAsync(httpEnv.Token.IfNone(env.Token)))
            ));
        
    public static Http<Stream> readContentAsStream(HttpResponseMessage message) =>
        new (
            new ReaderT<HttpEnv, IO, Stream>(httpEnv => liftIO(env => 
                message.Content.ReadAsStreamAsync(httpEnv.Token.IfNone(env.Token)))
            ));

    public static K<F, Uri> parseUri<F>(string url) where F : Fallible<F>, Applicative<F>
        => Try.lift(() => new Uri(url)).Match(F.Pure, F.Fail<Uri>);

    public static Http<Uri> parseUri(string url) => parseUri<Http>(url).As();

    public static Http<HttpResponseMessage> ensureSuccessStatus(HttpResponseMessage r) =>
        Try.lift(() =>
            {
                r.EnsureSuccessStatusCode();
                return r;
            })
            .Match(Applicative.pure<Http, HttpResponseMessage>,
                Fallible.error<Http, HttpResponseMessage>)
            .As();

    public static HttpClient client(Func<HttpRequestMessage, CancellationToken, HttpResponseMessage> sendAsync) =>
        new (new MessageHandler((m, t) => Task.FromResult(sendAsync(m, t))));
    
    public static HttpClient client(Func<HttpRequestMessage, HttpResponseMessage> sendAsync) =>
        new (new MessageHandler((m, t) => Task.FromResult(sendAsync(m))));
    
    public static HttpClient client(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> sendAsync) =>
        new (new MessageHandler(sendAsync));
    
    private class MessageHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> Run) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Run(request, cancellationToken);
    }
}