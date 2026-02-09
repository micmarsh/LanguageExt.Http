namespace LanguageExt;

public readonly record struct HttpEnv(HttpClient Client) : HasHttpClient; 

public interface HasHttpClient
{
    HttpClient Client { get; }
}