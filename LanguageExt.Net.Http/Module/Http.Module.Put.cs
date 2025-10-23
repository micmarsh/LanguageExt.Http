using System.Diagnostics.CodeAnalysis;
using LanguageExt.Traits;

namespace LanguageExt.Net;

public partial class Http
{
    public static K<M, HttpResponseMessage> put<M, Env>([StringSyntax("Uri")] string url, HttpContent content)
        where M : Readable<M, Env>, MonadIO<M>
        where Env : HasHttpClient
        =>  from uri in parseUri<IO>(url).As()
            from httpEnv in Readable.ask<M, Env>()
            from response in putAsIO(uri, content, httpEnv)
            select response;
        
    public static K<M, HttpResponseMessage> put<M, Env>(Uri url, HttpContent content)
        where M : Readable<M, Env>, MonadIO<M>
        where Env : HasHttpClient
        => Readable.ask<M, Env>().Bind(httpEnv => putAsIO(url, content, httpEnv));

    public static Http<HttpResponseMessage> put(Uri url, HttpContent content) =>
        put<Http, HttpEnv>(url, content).As();
    
    public static Http<HttpResponseMessage> put([StringSyntax("Uri")] string url, HttpContent content) =>
        put<Http, HttpEnv>(url, content).As();
    
    private static IO<HttpResponseMessage> putAsIO<Env>(Uri url, HttpContent content, Env httpEnv) 
        where Env : HasHttpClient  =>
        IO.liftAsync(env => httpEnv.Client.PutAsync(url, content, env.Token));
}