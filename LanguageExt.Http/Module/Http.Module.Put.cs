using System.Diagnostics.CodeAnalysis;
using LanguageExt.Traits;

namespace LanguageExt;

public partial class Http
{
    public static Http<HttpResponseMessage> put(Uri url, HttpContent content) =>
        Http<Http, HttpEnv>.put(url, content).As();
    
    public static Http<HttpResponseMessage> put([StringSyntax("Uri")] string url, HttpContent content) =>
        Http<Http, HttpEnv>.put(url, content).As();
    
    internal static IO<HttpResponseMessage> putAsIO<Env>(Uri url, HttpContent content, Env httpEnv) 
        where Env : HasHttpClient  =>
        IO.liftAsync(env => httpEnv.Client.PutAsync(url, content, env.Token));
}

public static partial class Http<M, Env>
{
    public static K<M, HttpResponseMessage> put([StringSyntax("Uri")] string url, HttpContent content)
        =>  from uri in parseUri(url)
            from httpEnv in Readable.ask<M, Env>()
            from response in Http.putAsIO(uri, content, httpEnv)
            select response;
    
    public static K<M, HttpResponseMessage> put(Uri url, HttpContent content)
        => Readable.ask<M, Env>() >> (httpEnv => Http.putAsIO(url, content, httpEnv));
}