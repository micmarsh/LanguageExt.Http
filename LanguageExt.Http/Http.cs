using LanguageExt.Common;
using LanguageExt.Traits;

namespace LanguageExt;

public record Http<A>(ReaderT<HttpEnv, IO, A> run) : Fallible<Http<A>, Http, Error, A>
{
    public IO<A> Run(HttpEnv env) => run.runReader(env).As();
    
    public IO<A> Run(Option<HttpClient> client = default, 
        Option<HttpCompletionOption> option = default,
        Option<CancellationToken> token = default) => 
        Run(new HttpEnv(client.IfNone(new HttpClient()), option, token)).As();

    public static implicit operator Http<A>(Fail<Error> fail) => Http.Fail<A>(fail.Value).As();

    public static implicit operator Http<A>(Pure<A> fail) => Applicative.pure<Http, A>(fail.Value).As();

    public static Http<A> operator |(Http<A> lhs, Http<A> rhs) => lhs.Catch(_ => true, _ => rhs).As();

    static Http<A> Fallible<Http<A>, Http, Error, A>.operator |(K<Http, A> lhs, Http<A> rhs) => lhs.Catch(_ => true, _ => rhs).As();

    static Http<A> Fallible<Http<A>, Http, Error, A>.operator |(Http<A> lhs, K<Http, A> rhs) => lhs.Catch(_ => true, _ => rhs).As();

    public static Http<A> operator |(Http<A> lhs, Pure<A> rhs) => lhs | (Http<A>)rhs;

    public static Http<A> operator |(Http<A> lhs, Fail<Error> rhs) => lhs | (Http<A>) rhs;

    public static Http<A> operator |(Http<A> lhs, CatchM<Error, Http, A> rhs) => lhs.Catch(rhs.Match, rhs.Action).As();
}
