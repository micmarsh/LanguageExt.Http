using System.Diagnostics.CodeAnalysis;
using LanguageExt.Traits;

namespace LanguageExt.Net;

public partial class Http
{
    public static K<M, HttpResponseMessage> get<M, Env>([StringSyntax("Uri")] string url, Option<HttpCompletionOption> option = default)
        where M : Readable<M, Env>, MonadIO<M>
        where Env : HasHttpClient, HasCompletionOption
        =>  from uri in parseUri<IO>(url).As()
            from httpEnv in Readable.ask<M, Env>()
            from response in getAsIO(uri, option, httpEnv)
            select response;
    
    public static K<M, HttpResponseMessage> get<M, Env>(Uri url, Option<HttpCompletionOption> option = default)
        where M : Readable<M, Env>, MonadIO<M>
        where Env : HasHttpClient, HasCompletionOption
        => Readable.ask<M, Env>().Bind(httpEnv => getAsIO(url, option, httpEnv));

    public static Http<HttpResponseMessage> get(Uri url, Option<HttpCompletionOption> option = default) =>
        get<Http, HttpEnv>(url, option).As();
    
    public static Http<HttpResponseMessage> get([StringSyntax("Uri")] string url, Option<HttpCompletionOption> option = default) =>
        get<Http, HttpEnv>(url, option).As();
    
    private static IO<HttpResponseMessage> getAsIO<Env>(Uri url, Option<HttpCompletionOption> option, Env httpEnv)
        where Env : HasHttpClient, HasCompletionOption =>
        IO.liftAsync(env => httpEnv.Client.GetAsync(url, resolveCompletion(option, httpEnv), env.Token));

    private static HttpCompletionOption resolveCompletion<Env>(Option<HttpCompletionOption> option, Env httpEnv)
        where Env : HasHttpClient, HasCompletionOption => 
        option.IfNone(httpEnv.CompletionOption.IfNone(HttpCompletionOption.ResponseContentRead));
}