using System.Diagnostics.CodeAnalysis;
using LanguageExt.Traits;

namespace LanguageExt.Net;

public partial class Http
{
    public static K<M, HttpResponseMessage> delete<M>([StringSyntax("Uri")] string url)
        where M : Readable<M, HttpEnv>, MonadIO<M>
        =>  from uri in parseUri<IO>(url).As()
            from httpEnv in ask<M>()
            from response in deleteAsIO(uri, httpEnv)
            select response;
        
    public static K<M, HttpResponseMessage> delete<M>(Uri url)
        where M : Readable<M, HttpEnv>, MonadIO<M>
        => ask<M>().Bind(httpEnv => deleteAsIO(url, httpEnv));

    public static Http<HttpResponseMessage> delete(Uri url) =>
        delete<Http>(url).As();
    
    public static Http<HttpResponseMessage> delete([StringSyntax("Uri")] string url) =>
        delete<Http>(url).As();
    
    private static IO<HttpResponseMessage> deleteAsIO(Uri url, HttpEnv httpEnv) =>
        IO.liftAsync(env => httpEnv.Client.DeleteAsync(url, env.Token));
}