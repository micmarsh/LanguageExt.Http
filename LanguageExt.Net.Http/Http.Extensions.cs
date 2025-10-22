using System.Runtime.CompilerServices;
using LanguageExt.Traits;

namespace LanguageExt.Net;

public static class HttpExt
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Http<A> As<A>(this K<Http, A> ma) => (Http<A>)ma;

    public static IO<A> Run<A>(this K<Http, A> ma, Option<HttpClient> client = default) => ma.As().Run(client);
}