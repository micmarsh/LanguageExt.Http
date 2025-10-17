using LanguageExt.Traits;

namespace LanguageExt;

public partial class Http
{
    public static K<M, HttpResponseMessage> get<M>(string url, Option<HttpCompletionOption> option = default)
        where M : Readable<M, HttpEnv>, MonadIO<M>
        => from httpEnv in ask<M>()
            from uri in parseUri<IO>(url).As()
            from response in getAsIO(uri, option, httpEnv)
            select response;
    
    public static K<M, HttpResponseMessage> get<M>(Uri url, Option<HttpCompletionOption> option = default)
        where M : Readable<M, HttpEnv>, MonadIO<M>
        => ask<M>().Bind(httpEnv => getAsIO(url, option, httpEnv));

    public static Http<HttpResponseMessage> get(Uri url, Option<HttpCompletionOption> option = default) =>
        get<Http>(url, option).As();
    
    public static Http<HttpResponseMessage> get(string url, Option<HttpCompletionOption> option = default) =>
        get<Http>(url, option).As();
    
    private static IO<HttpResponseMessage> getAsIO(Uri url, Option<HttpCompletionOption> option, HttpEnv httpEnv) =>
        IO.liftAsync(env => httpEnv.Client.GetAsync(url, resolveCompletion(option, httpEnv), env.Token));

    private static HttpCompletionOption resolveCompletion(Option<HttpCompletionOption> option, HttpEnv httpEnv) => 
        option.IfNone(httpEnv.CompletionOption.IfNone(HttpCompletionOption.ResponseContentRead));
}