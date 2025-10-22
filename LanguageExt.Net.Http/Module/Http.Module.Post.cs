using System.Diagnostics.CodeAnalysis;
using LanguageExt.Traits;

namespace LanguageExt.Net;

public partial class Http
{
    public static K<M, HttpResponseMessage> post<M>([StringSyntax("Uri")] string url, HttpContent content)
        where M : Readable<M, HttpEnv>, MonadIO<M>
        =>  from uri in parseUri<IO>(url).As()
            from httpEnv in ask<M>()
            from response in postAsIO(uri, content, httpEnv)
            select response;
        
    public static K<M, HttpResponseMessage> post<M>(Uri url, HttpContent content)
        where M : Readable<M, HttpEnv>, MonadIO<M>
        => ask<M>().Bind(httpEnv => postAsIO(url, content, httpEnv));

    public static Http<HttpResponseMessage> post(Uri url, HttpContent content) =>
        post<Http>(url, content).As();
    
    public static Http<HttpResponseMessage> post([StringSyntax("Uri")] string url, HttpContent content) =>
        post<Http>(url, content).As();
    
    private static IO<HttpResponseMessage> postAsIO(Uri url, HttpContent content, HttpEnv httpEnv) =>
        IO.liftAsync(env => httpEnv.Client.PostAsync(url, content, env.Token));
}