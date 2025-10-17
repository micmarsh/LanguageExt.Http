using System.Diagnostics.CodeAnalysis;
using LanguageExt.Traits;

namespace LanguageExt;

public partial class Http
{
    public static K<M, HttpResponseMessage> patch<M>([StringSyntax("Uri")] string url, HttpContent content)
        where M : Readable<M, HttpEnv>, MonadIO<M>
        =>  from uri in parseUri<IO>(url).As()
            from httpEnv in ask<M>()
            from response in patchAsIO(uri, content, httpEnv)
            select response;
        
    public static K<M, HttpResponseMessage> patch<M>(Uri url, HttpContent content)
        where M : Readable<M, HttpEnv>, MonadIO<M>
        => ask<M>().Bind(httpEnv => patchAsIO(url, content, httpEnv));

    public static Http<HttpResponseMessage> patch(Uri url, HttpContent content) =>
        patch<Http>(url, content).As();
    
    public static Http<HttpResponseMessage> patch([StringSyntax("Uri")] string url, HttpContent content) =>
        patch<Http>(url, content).As();
    
    private static IO<HttpResponseMessage> patchAsIO(Uri url, HttpContent content, HttpEnv httpEnv) =>
        IO.liftAsync(env => httpEnv.Client.PatchAsync(url, content, env.Token));
}