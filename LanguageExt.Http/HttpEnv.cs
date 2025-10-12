namespace LanguageExt;

public readonly record struct HttpEnv(
    HttpClient Client, 
    Option<HttpCompletionOption> CompletionOption,
    Option<CancellationToken> Token);