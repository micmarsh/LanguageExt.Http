using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using LanguageExt.Common;
using LanguageExt.Traits;
using static LanguageExt.Prelude;

namespace LanguageExt;

public partial class Http
{
    public static readonly Http<HttpEnv> ask = Readable.ask<Http, HttpEnv>().As();

    public static Http<A> Pure<A>(A item) => +Applicative.pure<Http, A>(item);
    
    public static K<M, string> @string<M>(HttpResponseMessage message)
        where M : MonadIO<M>
        => MonadIO.liftIO<M, string>(IO.liftAsync(env =>
            message.Content.ReadAsStringAsync(env.Token))
        );
    
    public static Http<string> @string(HttpResponseMessage message) => 
        @string<Http>(message).As();
        
    public static K<M, Stream> stream<M>(HttpResponseMessage message)
        where M : MonadIO<M>
        => MonadIO.liftIO<M, Stream>(IO.liftAsync(env =>
            message.Content.ReadAsStreamAsync(env.Token))
        );
    
    public static Http<Stream> stream(HttpResponseMessage message) => 
        stream<Http>(message).As();

    public static K<F, Uri> parseUri<F>(string url) where F : Fallible<F>, Applicative<F>
        =>
            @try<F, Uri>(() => new Uri(url));

    public static Http<Uri> parseUri(string url) => parseUri<Http>(url).As();

    public static K<F, HttpResponseMessage> ensureSuccessStatus<F>(HttpResponseMessage r)
        where F : Fallible<F>, Applicative<F>
        => @try<F, HttpResponseMessage>(() =>
        {
            r.EnsureSuccessStatusCode();
            return r;
        });
    
    public static Http<HttpResponseMessage> ensureSuccessStatus(HttpResponseMessage r) =>
        ensureSuccessStatus<Http>(r).As();

    public static HttpContent content(string value) =>
        new ByteArrayContent(Encoding.ASCII.GetBytes(value));
    
    public static HttpContent content(JsonRequestWrapper value) => 
        JsonContent.Create(value.Value, options: GlobalJsonConfig.Options);
    
    public static Http<A> @try<A>(Func<A> run) => +@try<Http, A>(run);
    
    public static K<M, A> @try<M, A>(Func<A> run) where M : Applicative<M>, Fallible<M>
        => Try.lift(run).Match(M.Pure, M.Fail<A>);
}

public static partial class Http<M, Env>
    where M : Readable<M, Env>, MonadIO<M>, Fallible<M>
    where Env : HasHttpClient
{
    public static K<M, string> readContentAsString(HttpResponseMessage message)
        => Http.@string<M>(message);

    public static K<M, Stream> stream(HttpResponseMessage message)
        => Http.stream<M>(message);

    public static K<M, Uri> parseUri(string url)
        => @try(() => new Uri(url));
    
    public static K<M, HttpResponseMessage> ensureSuccessStatus(HttpResponseMessage r)
        => @try(() =>
        {
            r.EnsureSuccessStatusCode();
            return r;
        });
    
    public static HttpContent content(string value) => Http.content(value);

    public static HttpContent content(JsonRequestWrapper value) => Http.content(value);

    public static K<M, A> @try<A>(Func<A> run) => Http.@try<M, A>(run);
}

public readonly record struct JsonRequestWrapper(object? Value);
