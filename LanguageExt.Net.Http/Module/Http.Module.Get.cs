using System.Diagnostics.CodeAnalysis;
using LanguageExt.Traits;

namespace LanguageExt.Net;

public partial class Http
{
    public static K<M, HttpResponseMessage> get<M, Env>([StringSyntax("Uri")] string url, HttpCompletionOption option = HttpCompletionOption.ResponseContentRead)
        where M : Readable<M, Env>, MonadIO<M>
        where Env : HasHttpClient
        =>  from uri in parseUri<IO>(url).As()
            from httpEnv in Readable.ask<M, Env>()
            from response in getAsIO(uri, option, httpEnv)
            select response;
    
    public static K<M, HttpResponseMessage> get<M, Env>(Uri url, HttpCompletionOption option = HttpCompletionOption.ResponseContentRead)
        where M : Readable<M, Env>, MonadIO<M>
        where Env : HasHttpClient
        => Readable.ask<M, Env>().Bind(httpEnv => getAsIO(url, option, httpEnv));

    public static Http<HttpResponseMessage> get(Uri url, HttpCompletionOption option = HttpCompletionOption.ResponseContentRead) =>
        get<Http, HttpEnv>(url, option).As();
    
    public static Http<HttpResponseMessage> get([StringSyntax("Uri")] string url, HttpCompletionOption option = HttpCompletionOption.ResponseContentRead) =>
        get<Http, HttpEnv>(url, option).As();
    
    private static IO<HttpResponseMessage> getAsIO<Env>(Uri url, HttpCompletionOption option, Env httpEnv)
        where Env : HasHttpClient =>
        IO.liftAsync(env => httpEnv.Client.GetAsync(url, option, env.Token));
}