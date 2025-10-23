namespace LanguageExt.Net;

public readonly record struct HttpEnv(
    HttpClient Client,
    Option<HttpCompletionOption> CompletionOption)
    : HasCompletionOption, HasHttpClient ; 

public interface HasHttpClient
{
    HttpClient Client { get; }
}

public interface HasCompletionOption
{
    Option<HttpCompletionOption> CompletionOption { get; }
}