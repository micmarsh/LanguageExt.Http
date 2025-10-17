using LanguageExt.Traits;
using static LanguageExt.Prelude;

namespace LanguageExt;

public partial class Http
{
    public static K<M, HttpEnv> ask<M>() where M : Readable<M, HttpEnv> => Readable.ask<M, HttpEnv>();

    public static K<M, string> readContentAsString<M>(HttpResponseMessage message)
        where M : Readable<M, HttpEnv>, MonadIO<M>
        => ask<M>().Bind(httpEnv => liftIO(env =>
            message.Content.ReadAsStringAsync(env.Token))
        );
    
    public static Http<string> readContentAsString(HttpResponseMessage message) => 
        readContentAsString<Http>(message).As();
        
    public static K<M, Stream> readContentAsStream<M>(HttpResponseMessage message)
        where M : Readable<M, HttpEnv>, MonadIO<M>
        => ask<M>().Bind(httpEnv => liftIO(env =>
            message.Content.ReadAsStreamAsync(env.Token))
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
    
}