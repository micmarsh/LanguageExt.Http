using System.Diagnostics.CodeAnalysis;
using LanguageExt.Traits;

namespace LanguageExt;

public partial class Http
{
    public static K<M, HttpResponseMessage> post<M, Env>([StringSyntax("Uri")] string url, HttpContent content)
        where M : Readable<M, Env>, MonadIO<M>
        where Env : HasHttpClient
        =>  from uri in parseUri<IO>(url).As()
            from httpEnv in Readable.ask<M, Env>()
            from response in postAsIO(uri, content, httpEnv)
            select response;
        
    public static K<M, HttpResponseMessage> post<M, Env>(Uri url, HttpContent content)
        where M : Readable<M, Env>, MonadIO<M>
        where Env : HasHttpClient
        => Readable.ask<M, Env>().Bind(httpEnv => postAsIO(url, content, httpEnv));

    public static Http<HttpResponseMessage> post(Uri url, HttpContent content) =>
        post<Http, HttpEnv>(url, content).As();
    
    public static Http<HttpResponseMessage> post([StringSyntax("Uri")] string url, HttpContent content) =>
        post<Http, HttpEnv>(url, content).As();
    
    private static IO<HttpResponseMessage> postAsIO<Env>(Uri url, HttpContent content, Env httpEnv) 
        where Env : HasHttpClient  =>
        IO.liftAsync(env => httpEnv.Client.PostAsync(url, content, env.Token));
}