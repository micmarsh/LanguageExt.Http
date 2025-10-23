// See https://aka.ms/new-console-template for more information

using System.Net;
using System.Net.Http.Json;
using System.Text;
using LanguageExt;
using LanguageExt.Net;
using LanguageExt.Traits;
using Polly;
using static LanguageExt.Net.Http;
using static LanguageExt.Prelude;
using Http = LanguageExt.Http;

var testClient = Http.client(request =>
{
    var path = request.RequestUri.PathAndQuery;
    return request.Method.Method switch
    {
        "GET" when path.Contains("users") => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(Encoding.ASCII.GetBytes("1,2,3,4,5"))
        },
        "GET" when path.Contains("user/") => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(Encoding.ASCII.GetBytes($"User-{int.Parse(path.Split("/").Last())}"))
        },
        "POST" => new HttpResponseMessage(HttpStatusCode.OK),
        _ => throw new InvalidOperationException(request.Method.Method)
    };
});

IEnumerable<UserId> ParseUserIds(string ids) => ids.Split(",").Select(int.Parse).Select(i => new UserId(i));
User ParseUser(string serialized) => new User(serialized, int.Parse(serialized.Split("-").Last()));
string SerializeUser(User user) => user.ToString();

const int maxRetryAttempts = 3;

var retryPolicy = Policy<HttpResponseMessage>
    .Handle<Exception>()
    .WaitAndRetryAsync(maxRetryAttempts, attempt =>
        TimeSpan.FromSeconds(attempt));

Task<HttpResponseMessage> UpdateUser(HttpClient client, User user, CancellationToken token)
{
    var updatedUser = user with { Name = user.Name + "-Updated" };
    var request = content(SerializeUser(updatedUser));

    Task<HttpResponseMessage> TryPostAsync() => 
        client.PostAsync($"http://api.url/user/{user.Id}", request, token);

    return retryPolicy.ExecuteAsync(TryPostAsync);
}

async Task<IEnumerable<UserId>> GetAllUserIds(HttpClient httpClient, CancellationToken cancellationToken)
{
    var userIdResponse = await httpClient.GetAsync("http://api.url/users", cancellationToken);
    return ParseUserIds(await userIdResponse.Content.ReadAsStringAsync(cancellationToken));
}

async Task<User[]> GetAllUsers(IEnumerable<UserId> enumerable, HttpClient client, CancellationToken token)
{
    var fullUserResponseTasks = enumerable.Select<UserId, Task<HttpResponseMessage>>(id => client.GetAsync($"http://api.url/user/{id.Value}", token));
    var fullUserResponses = await Task.WhenAll(fullUserResponseTasks);
    var fullUserTasks =
        fullUserResponses.Select(async resp => ParseUser(await resp.Content.ReadAsStringAsync(token)));
    var users = await Task.WhenAll(fullUserTasks);
    return users;
}

async Task UpdateAllUsers(HttpClient client, CancellationToken token)
{
    try
    {
        var userIds = await GetAllUserIds(client, token);
        var fullUsers = await GetAllUsers(userIds, client, token);
        await Task.WhenAll(fullUsers.Select(user => UpdateUser(client, user, token)));
    }
    catch (Exception e)
    {
        Console.WriteLine($"Error updating all users: {e}");
    }
}

await UpdateAllUsers(testClient, default);

var retrySchedule = Schedule.linear(1.Seconds()).Take(maxRetryAttempts);

var getAllUserIds = get("http://api.url/users").Bind(readContentAsString).Map(ParseUserIds);

Http<User> getFullUser(UserId userId) => 
    get($"http://api.url/user/{userId.Value}").Bind(readContentAsString)
        .Map(ParseUser);

Http<HttpResponseMessage> updateUser(User user) => 
    post($"http://api.url/user/{user.Id}",
        content(SerializeUser(user with {Name = user.Name + "-Updated"})));

var updateAllUsers =
    from userIds in getAllUserIds
    from users in userIds.AsIterable().Traverse(getFullUser)
    from _ in users.Traverse(u => updateUser(u).RetryIO(retrySchedule))
    select unit;

await updateAllUsers
    .Run(testClient)
    .Catch(err =>
    {
        Console.WriteLine($"Error updating all users: {err.ToException()}");
        return unit;
    })
    .RunAsync(EnvIO.New(token: default));

    

public record UserId(int Value);
public record User(string Name, int Id);


public record Config(int MagicNumber, HttpEnv Http, string MagicText)
    : HasHttpClient
{
    public HttpClient Client => Http.Client; 
}

public record MyApp<A>(ReaderT<Config, IO, A> run) : K<MyApp, A>;

public class MyApp: Deriving.MonadUnliftIO<MyApp, ReaderT<Config, IO>>,
    Deriving.Readable<MyApp, Config, ReaderT<Config, IO>>
{
    public static K<ReaderT<Config, IO>, A> Transform<A>(K<MyApp, A> fa)
    {
        throw new NotImplementedException();
    }

    public static K<MyApp, A> CoTransform<A>(K<ReaderT<Config, IO>, A> fa)
    {
        throw new NotImplementedException();
    }
}



