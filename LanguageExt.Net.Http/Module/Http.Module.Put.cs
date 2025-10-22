using System.Diagnostics.CodeAnalysis;
using LanguageExt.Traits;

namespace LanguageExt.Net;

public partial class Http
{
    public static K<M, HttpResponseMessage> put<M>([StringSyntax("Uri")] string url, HttpContent content)
        where M : Readable<M, HttpEnv>, MonadIO<M>
        =>  from uri in parseUri<IO>(url).As()
            from httpEnv in ask<M>()
            from response in putAsIO(uri, content, httpEnv)
            select response;
        
    public static K<M, HttpResponseMessage> put<M>(Uri url, HttpContent content)
        where M : Readable<M, HttpEnv>, MonadIO<M>
        => ask<M>().Bind(httpEnv => putAsIO(url, content, httpEnv));

    public static Http<HttpResponseMessage> put(Uri url, HttpContent content) =>
        put<Http>(url, content).As();
    
    public static Http<HttpResponseMessage> put([StringSyntax("Uri")] string url, HttpContent content) =>
        put<Http>(url, content).As();
    
    private static IO<HttpResponseMessage> putAsIO(Uri url, HttpContent content, HttpEnv httpEnv) =>
        IO.liftAsync(env => httpEnv.Client.PutAsync(url, content, env.Token));
}