using System.Runtime.CompilerServices;
using LanguageExt.Traits;

namespace LanguageExt.Net;

public static class HttpExt
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Http<A> As<A>(this K<Http, A> ma) => (Http<A>)ma;

    public static IO<A> Run<A>(this K<Http, A> ma, Option<HttpClient> client = default) => ma.As().Run(client);

    public static Http<C> SelectMany<A, B, C>(this K<Http, A> ma, Func<A, Http<B>> bind, Func<A, B, C> project) =>
        ma.As().SelectMany(bind, project);
    
    public static Http<C> SelectMany<A, B, C>(this Ask<HttpEnv, A> ask, Func<A, Http<B>> bind, Func<A, B, C> project) =>
        ((Http<A>)ask).SelectMany(bind, project);
}