using System.Diagnostics.CodeAnalysis;
using LanguageExt.Traits;

namespace LanguageExt;

public partial class Http
{
    public static K<M, HttpResponseMessage> send<M, Env>(HttpRequestMessage request, HttpCompletionOption option = HttpCompletionOption.ResponseContentRead)
        where M : Readable<M, Env>, MonadIO<M>
        where Env : HasHttpClient
        => Readable.ask<M, Env>() >> (httpEnv => sendAsIO(request, option, httpEnv));

    public static Http<HttpResponseMessage> send(HttpRequestMessage request, HttpCompletionOption option = HttpCompletionOption.ResponseContentRead) =>
        send<Http, HttpEnv>(request, option).As();
    
    private static IO<HttpResponseMessage> sendAsIO<Env>(HttpRequestMessage request, HttpCompletionOption option, Env httpEnv)
        where Env : HasHttpClient
        => IO.liftAsync(env => httpEnv.Client.SendAsync(request, option, env.Token));
}