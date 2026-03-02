using System.Diagnostics.CodeAnalysis;
using LanguageExt.Traits;

namespace LanguageExt;

public partial class Http
{
    public static Http<HttpResponseMessage> send(HttpRequestMessage request, HttpCompletionOption option = HttpCompletionOption.ResponseContentRead) =>
        Http<Http, HttpEnv>.send(request, option).As();
    
    internal static IO<HttpResponseMessage> sendAsIO<Env>(HttpRequestMessage request, HttpCompletionOption option, Env httpEnv)
        where Env : HasHttpClient
        => IO.liftAsync(env => httpEnv.Client.SendAsync(request, option, env.Token));
}

public static partial class Http<M, Env>
{
    public static K<M, HttpResponseMessage> send(HttpRequestMessage request, HttpCompletionOption option = HttpCompletionOption.ResponseContentRead)
        => Readable.ask<M, Env>() >> (httpEnv => Http.sendAsIO(request, option, httpEnv));
}
