namespace LanguageExt;

public partial class Http
{
    public static HttpClient client(Func<HttpRequestMessage, CancellationToken, HttpResponseMessage> sendAsync) =>
        new (new MessageHandler((m, t) => Task.FromResult(sendAsync(m, t))));
    
    public static HttpClient client(Func<HttpRequestMessage, HttpResponseMessage> sendAsync) =>
        new (new MessageHandler((m, t) => Task.FromResult(sendAsync(m))));
    
    public static HttpClient client(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> sendAsync) =>
        new (new MessageHandler(sendAsync));
    
    private class MessageHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> Run) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Run(request, cancellationToken);
    }
}