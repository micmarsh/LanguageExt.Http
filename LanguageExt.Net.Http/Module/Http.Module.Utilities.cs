using System.Text;
using System.Text.Json;
using LanguageExt.Common;
using LanguageExt.Traits;
using static LanguageExt.Prelude;

namespace LanguageExt;

public partial class Http
{
    public static readonly Http<HttpEnv> ask = Readable.ask<Http, HttpEnv>().As();

    public static Http<A> Pure<A>(A item) => new(new ReaderT<HttpEnv, IO, A>(_ => IO.pure(item)));
    
    public static K<M, string> readContentAsString<M>(HttpResponseMessage message)
        where M : MonadIO<M>
        => MonadIO.liftIO<M, string>(IO.liftAsync(env =>
            message.Content.ReadAsStringAsync(env.Token))
        );
    
    public static Http<string> readContentAsString(HttpResponseMessage message) => 
        readContentAsString<Http>(message).As();
        
    public static K<M, Stream> readContentAsStream<M>(HttpResponseMessage message)
        where M : MonadIO<M>
        => MonadIO.liftIO<M, Stream>(IO.liftAsync(env =>
            message.Content.ReadAsStreamAsync(env.Token))
        );
    
    public static Http<Stream> readContentAsStream(HttpResponseMessage message) => 
        readContentAsStream<Http>(message).As();

    public static K<F, Uri> parseUri<F>(string url) where F : Fallible<F>, Applicative<F>
        => @try<F, Uri>(() => new Uri(url));

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
    
    public static K<M, A> @try<M, A>(Func<A> run) where M : Applicative<M>, Fallible<M>
        => Try.lift(run).Match(M.Pure, M.Fail<A>);

    public static Http<A> @try<A>(Func<A> run) => +@try<Http, A>(run);

    public static Http<JsonElement> json(HttpResponseMessage r) =>
        readContentAsStream(r) >> (stream => @try(() => JsonDocument.Parse(stream).RootElement));
    
    public static Http<Result> json<Result>(HttpResponseMessage r) =>
        from str in readContentAsString(r)
        from resultNull in @try(() => JsonSerializer.Deserialize<Result>(str))
        from result in Optional(resultNull).Match(Pure, () => 
            Fail<Result>(Error.New($"Could not deserialize json result {str}")))
        select result;
}