using LanguageExt.Traits;

namespace LanguageExt;

public record Http<A>(ReaderT<HttpEnv, IO, A> run) : K<Http, A>
{
    public IO<A> Run(HttpEnv env) => run.runReader(env).As();
    
    public IO<A> Run(Option<HttpClient> client = default, 
        Option<HttpCompletionOption> option = default,
        Option<CancellationToken> token = default) => 
        Run(new HttpEnv(client.IfNone(new HttpClient()), option, token)).As();
}
