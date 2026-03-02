using System.Diagnostics.CodeAnalysis;
using LanguageExt.Traits;

namespace LanguageExt;

public partial class Http
{
    public static Http<HttpResponseMessage> get(Uri url, HttpCompletionOption option = HttpCompletionOption.ResponseContentRead) =>
        Http<Http, HttpEnv>.get(url, option).As();
    
    public static Http<HttpResponseMessage> get([StringSyntax("Uri")] string url, HttpCompletionOption option = HttpCompletionOption.ResponseContentRead) =>
        Http<Http, HttpEnv>.get(url, option).As();
    
    internal static IO<HttpResponseMessage> getAsIO<Env>(Uri url, HttpCompletionOption option, Env httpEnv)
        where Env : HasHttpClient =>
        IO.liftAsync(env => httpEnv.Client.GetAsync(url, option, env.Token));
}

public static partial class Http<M, Env>
    where M : Readable<M, Env>, MonadIO<M>
    where Env : HasHttpClient
{
    public static K<M, HttpResponseMessage> get([StringSyntax("Uri")] string url, HttpCompletionOption option = HttpCompletionOption.ResponseContentRead)
        =>  from uri in Http.parseUri<IO>(url).As()
            from httpEnv in Readable.ask<M, Env>()
            from response in Http.getAsIO(uri, option, httpEnv)
            select response;
    
    public static K<M, HttpResponseMessage> get(Uri url, HttpCompletionOption option = HttpCompletionOption.ResponseContentRead)
       => Readable.ask<M, Env>() >> (httpEnv => Http.getAsIO(url, option, httpEnv));
}
