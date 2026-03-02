using System.Diagnostics.CodeAnalysis;
using LanguageExt.Traits;

namespace LanguageExt;

public partial class Http
{
    public static Http<HttpResponseMessage> delete(Uri url) =>
        Http<Http, HttpEnv>.delete(url).As();
    
    public static Http<HttpResponseMessage> delete([StringSyntax("Uri")] string url) =>
        Http<Http, HttpEnv>.delete(url).As();
    
    internal static IO<HttpResponseMessage> deleteAsIO<Env>(Uri url, Env httpEnv) 
        where Env : HasHttpClient  =>
        IO.liftAsync(env => httpEnv.Client.DeleteAsync(url, env.Token));
}

public static partial class Http<M, Env>
{
    public static K<M, HttpResponseMessage> delete([StringSyntax("Uri")] string url)
        =>  from uri in parseUri(url)
            from httpEnv in Readable.ask<M, Env>()
            from response in Http.deleteAsIO(uri, httpEnv)
            select response;
    
    public static K<M, HttpResponseMessage> delete(Uri url)
        => Readable.ask<M, Env>() >> (httpEnv => Http.deleteAsIO(url, httpEnv));
}
