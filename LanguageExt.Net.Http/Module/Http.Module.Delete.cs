using System.Diagnostics.CodeAnalysis;
using LanguageExt.Traits;

namespace LanguageExt.Net;

public partial class Http
{
    public static K<M, HttpResponseMessage> delete<M, Env>([StringSyntax("Uri")] string url)
        where M : Readable<M, Env>, MonadIO<M>
        where Env : HasHttpClient
        =>  from uri in parseUri<IO>(url).As()
            from httpEnv in Readable.ask<M, Env>()
            from response in deleteAsIO(uri, httpEnv)
            select response;
        
    public static K<M, HttpResponseMessage> delete<M, Env>(Uri url)
        where M : Readable<M, Env>, MonadIO<M>
        where Env : HasHttpClient
        => Readable.ask<M, Env>().Bind(httpEnv => deleteAsIO(url, httpEnv));

    public static Http<HttpResponseMessage> delete(Uri url) =>
        delete<Http, HttpEnv>(url).As();
    
    public static Http<HttpResponseMessage> delete([StringSyntax("Uri")] string url) =>
        delete<Http, HttpEnv>(url).As();
    
    private static IO<HttpResponseMessage> deleteAsIO<Env>(Uri url, Env httpEnv) 
        where Env : HasHttpClient  =>
        IO.liftAsync(env => httpEnv.Client.DeleteAsync(url, env.Token));
}