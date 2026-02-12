using LanguageExt.Common;
using LanguageExt.Traits;

namespace LanguageExt;

public record Http<A>(ReaderT<HttpEnv, IO, A> run) : K<Http, A>
{
    /// <summary>
    /// "Runs" the Http action, returning an IO.
    /// If you need to thread a <see cref="CancellationToken"/> through your computation,
    /// utilize the returned <see cref="IO"/>'s <see cref="EnvIO"/> when calling its Run or RunAsync
    /// </summary>
    public IO<A> RunIO(HttpEnv env) => run.runReader(env).As();
    
    /// <summary>
    /// "Runs" the Http action, returning an IO.
    /// If you need to thread a <see cref="CancellationToken"/> through your computation,
    /// utilize the returned <see cref="IO"/>'s <see cref="EnvIO"/> when calling its Run or RunAsync
    /// </summary>
    /// <param name="client"></param>
    /// <param name="option"></param>
    /// <returns></returns>
    public IO<A> RunIO(Option<HttpClient> client = default) => 
        RunIO(new HttpEnv(client.IfNone(new HttpClient()))).As();

    public Http<B> Map<B>(Func<A, B> f) => this.Kind().Map(f).As();
    public Http<B> Select<B>(Func<A, B> f) => Map(f);

    public Http<B> Bind<B>(Func<A, Http<B>> bind) => this.Kind().Bind(bind).As();

    public Http<C> SelectMany<B, C>(Func<A, Http<B>> bind, Func<A, B, C> project) =>
        Bind(a => bind(a).Map(b => project(a, b)));
    
    public Http<B> Bind<B>(Func<A, Ask<HttpEnv, B>> bind) => this.Kind().Bind(a => (Http<B>) bind(a)).As();

    public Http<C> SelectMany<B, C>(Func<A, Ask<HttpEnv, B>> bind, Func<A, B, C> project) =>
        Bind(a => ((Http<B>)bind(a)).Map(b => project(a, b)));
    
    public Http<C> SelectMany<B, C>(Func<A, K<Http, B>> bind, Func<A, B, C> project) =>
        SelectMany(a => bind(a).As(), project);

    public Http<C> SelectMany<B, C>(Func<A, IO<B>> bind, Func<A, B, C> project) =>
        Bind(a => MonadIO.liftIO<Http, B>(bind(a)).Map(b => project(a, b)).As());
    
    public Http<C> SelectMany<B, C>(Func<A, K<IO, B>> bind, Func<A, B, C> project) =>
        SelectMany(a => bind(a).As(), project);
    
    public static implicit operator Http<A>(Fail<Error> fail) => Http.Fail<A>(fail.Value).As();

    public static implicit operator Http<A>(Pure<A> fail) => Applicative.pure<Http, A>(fail.Value).As();

    public static Http<A> operator |(Http<A> lhs, Http<A> rhs) => lhs.Catch(_ => true, _ => rhs).As();
    
    public static Http<A> operator |(Http<A> lhs, Pure<A> rhs) => lhs | (Http<A>)rhs;

    public static Http<A> operator |(Http<A> lhs, Fail<Error> rhs) => lhs | (Http<A>) rhs;

    public static Http<A> operator |(Http<A> lhs, CatchM<Error, Http, A> rhs) => lhs.Catch(rhs.Match, rhs.Action).As();

    public static implicit operator Http<A>(Ask<HttpEnv, A> ask) => new (ask.ToReaderT<IO>());
}
