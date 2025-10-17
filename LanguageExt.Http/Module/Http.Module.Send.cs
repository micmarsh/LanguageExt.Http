using System.Diagnostics.CodeAnalysis;
using LanguageExt.Traits;

namespace LanguageExt;

public partial class Http
{
    public static K<M, HttpResponseMessage> send<M>(HttpRequestMessage request, Option<HttpCompletionOption> option = default)
        where M : Readable<M, HttpEnv>, MonadIO<M>
        => ask<M>().Bind(httpEnv => sendAsIO(request, option, httpEnv));

    public static Http<HttpResponseMessage> send(HttpRequestMessage request, Option<HttpCompletionOption> option = default) =>
        send<Http>(request, option).As();
    
    private static IO<HttpResponseMessage> sendAsIO(HttpRequestMessage request, Option<HttpCompletionOption> option, HttpEnv httpEnv) =>
        IO.liftAsync(env => httpEnv.Client.SendAsync(request, resolveCompletion(option, httpEnv), env.Token));
}