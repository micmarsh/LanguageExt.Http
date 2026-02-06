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
    
    public static K<M, string> readContentAsString<M>(HttpResponseMessage message)
        where M : MonadIO<M>
        => MonadIO.liftIO<M, string>(IO.liftAsync(env =>
            message.Content.ReadAsStringAsync(env.Token))
        );
    
    public static Http<string> @string(HttpResponseMessage message) => 
        readContentAsString<Http>(message).As();
        
    public static K<M, Stream> stream<M>(HttpResponseMessage message)
        where M : MonadIO<M>
        => MonadIO.liftIO<M, Stream>(IO.liftAsync(env =>
            message.Content.ReadAsStreamAsync(env.Token))
        );
    
    public static Http<Stream> stream(HttpResponseMessage message) => 
        stream<Http>(message).As();

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
    
    public static HttpContent content(JsonWrapper value) => JsonContent.Create(value);

    public record JsonWrapper(object? Value);
    
    public static K<M, A> @try<M, A>(Func<A> run) where M : Applicative<M>, Fallible<M>
        => Try.lift(run).Match(M.Pure, M.Fail<A>);

    public static Http<A> @try<A>(Func<A> run) => +@try<Http, A>(run);

    
    public static Http<JsonElement> parse(Stream stream) => @try(() => JsonDocument.Parse(stream).RootElement);
    public static Http<JsonElement> parse(string str) => @try(() => JsonDocument.Parse(str).RootElement);
    
    public static Http<Result> deserialize<Result>(Stream stream) => +deserialize<Http, Result>(stream);
    public static Http<Result> deserialize<Result>(string str) => +deserialize<Http, Result>(str);
    public static Http<Result> deserialize<Result>(JsonElement json) => +deserialize<Http, Result>(json);


    // todo totally separate lib with these generics, maybe with `Fin` default, and way better error messages
    public static K<F, JsonElement> parse<F>(Stream stream) 
        where F : Fallible<F>, Applicative<F> 
        => @try<F, JsonElement>(() => JsonDocument.Parse(stream).RootElement);
    public static K<F, JsonElement> parse<F>(String str) 
        where F : Fallible<F>, Applicative<F> 
        => @try<F, JsonElement>(() => JsonDocument.Parse(str).RootElement);

    public static K<F, Result> deserialize<F, Result>(JsonElement json)
        where F : Fallible<F>, Monad<F>
        =>  from resultNull in @try<F, Result>(() => json.Deserialize<Result>())
            from result in Optional(resultNull).Match(F.Pure, () => 
                F.Fail<Result>(Error.New($"Could not convert json element {json.ValueKind} to {typeof(Result).Name}")))
            select result;
    
    public static K<F, Result> deserialize<F, Result>(Stream stream)
        where F : Fallible<F>, Monad<F>
        =>
            from resultNull in @try<F, Result>(() => JsonSerializer.Deserialize<Result>(stream))
            from result in Optional(resultNull).Match(F.Pure, () => 
                F.Fail<Result>(Error.New($"Could not deserialize json stream result")))
            select result;
    
    public static K<F, Result> deserialize<F, Result>(string str)
        where F : Fallible<F>, Monad<F>
        =>
        from resultNull in @try<F, Result>(() => JsonSerializer.Deserialize<Result>(str))
        from result in Optional(resultNull).Match(F.Pure, () => 
            F.Fail<Result>(Error.New($"Could not deserialize json result {str}")))
        select result;
}