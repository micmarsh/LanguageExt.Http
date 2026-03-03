using System.Diagnostics.CodeAnalysis;
using LanguageExt.Traits;

namespace LanguageExt;

public partial class Http
{
    public static Http<HttpResponseMessage> patch(Uri url, HttpContent content) =>
        Http<Http, HttpEnv>.patch(url, content).As();
    
    public static Http<HttpResponseMessage> patch([StringSyntax("Uri")] string url, HttpContent content) =>
        Http<Http, HttpEnv>.patch(url, content).As();
    
    internal static IO<HttpResponseMessage> patchAsIO<Env>(Uri url, HttpContent content, Env httpEnv) 
        where Env : HasHttpClient  =>
        IO.liftAsync(env => httpEnv.Client.PatchAsync(url, content, env.Token));
}

public static partial class Http<M, Env>
{
    public static K<M, HttpResponseMessage> patch([StringSyntax("Uri")] string url, HttpContent content)
        =>  from uri in parseUri(url)
            from httpEnv in Readable.ask<M, Env>()
            from response in Http.patchAsIO(uri, content, httpEnv)
            select response;
    
    public static K<M, HttpResponseMessage> patch(Uri url, HttpContent content)
        => Readable.ask<M, Env>() >> (httpEnv => Http.patchAsIO(url, content, httpEnv));
}