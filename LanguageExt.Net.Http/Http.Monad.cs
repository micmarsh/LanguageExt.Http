using LanguageExt.Common;
using LanguageExt.Traits;

namespace LanguageExt;

public partial class Http :
    Deriving.Readable<Http, HttpEnv, ReaderT<HttpEnv, IO>>,
    Deriving.MonadUnliftIO<Http,  ReaderT<HttpEnv, IO>>,
    Deriving.Choice<Http,  ReaderT<HttpEnv, IO>>,
    Fallible<Http>
{
    public static K<ReaderT<HttpEnv, IO>, A> Transform<A>(K<Http, A> fa) => fa.As().run;
    public static K<Http, A> CoTransform<A>(K<ReaderT<HttpEnv, IO>, A> fa) => new Http<A>(fa.As());
    public static K<Http, A> Fail<A>(Error error) => new Http<A>(
        ReaderT<HttpEnv, IO, A>.LiftIO(IO.fail<A>(error))
    );
    public static K<Http, A> Catch<A>(K<Http, A> fa, Func<Error, bool> Predicate, Func<Error, K<Http, A>> Fail) => new Http<A>(
        new ReaderT<HttpEnv, IO, A>(
            c => fa.As().run.runReader(c)
                .Catch(Predicate, err => Fail(err).As().run.runReader(c)))
    );
    
}