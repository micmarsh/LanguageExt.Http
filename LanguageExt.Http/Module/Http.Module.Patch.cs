using System.Diagnostics.CodeAnalysis;
using LanguageExt.Traits;

namespace LanguageExt;

public partial class Http
{
    public static K<M, HttpResponseMessage> patch<M, Env>([StringSyntax("Uri")] string url, HttpContent content)
        where M : Readable<M, Env>, MonadIO<M>
        where Env : HasHttpClient
        =>  from uri in parseUri<IO>(url).As()
            from httpEnv in Readable.ask<M, Env>()
            from response in patchAsIO(uri, content, httpEnv)
            select response;
        
    public static K<M, HttpResponseMessage> patch<M, Env>(Uri url, HttpContent content)
        where M : Readable<M, Env>, MonadIO<M>
        where Env : HasHttpClient
        => Readable.ask<M, Env>() >> (httpEnv => patchAsIO(url, content, httpEnv));

    public static Http<HttpResponseMessage> patch(Uri url, HttpContent content) =>
        patch<Http, HttpEnv>(url, content).As();
    
    public static Http<HttpResponseMessage> patch([StringSyntax("Uri")] string url, HttpContent content) =>
        patch<Http, HttpEnv>(url, content).As();
    
    private static IO<HttpResponseMessage> patchAsIO<Env>(Uri url, HttpContent content, Env httpEnv) 
        where Env : HasHttpClient  =>
        IO.liftAsync(env => httpEnv.Client.PatchAsync(url, content, env.Token));
}