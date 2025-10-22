namespace LanguageExt.Net;

public readonly record struct HttpEnv(
    HttpClient Client, 
    Option<HttpCompletionOption> CompletionOption);