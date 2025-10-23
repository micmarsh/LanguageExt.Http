using System.Diagnostics.CodeAnalysis;
using LanguageExt.Traits;

namespace LanguageExt.Net;

public partial class Http
{
    public static K<M, HttpResponseMessage> send<M, Env>(HttpRequestMessage request, Option<HttpCompletionOption> option = default)
        where M : Readable<M, Env>, MonadIO<M>
        where Env : HasHttpClient, HasCompletionOption
        => Readable.ask<M, Env>().Bind(httpEnv => sendAsIO<Env>(request, option, httpEnv));

    public static Http<HttpResponseMessage> send(HttpRequestMessage request, Option<HttpCompletionOption> option = default) =>
        send<Http, HttpEnv>(request, option).As();
    
    private static IO<HttpResponseMessage> sendAsIO<Env>(HttpRequestMessage request, Option<HttpCompletionOption> option, Env httpEnv)
        where Env : HasHttpClient, HasCompletionOption
        => IO.liftAsync(env => httpEnv.Client.SendAsync(request, resolveCompletion(option, httpEnv), env.Token));
}