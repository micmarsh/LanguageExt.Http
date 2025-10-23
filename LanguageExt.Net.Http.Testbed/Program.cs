// See https://aka.ms/new-console-template for more information

using System.Net;
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

async Task<User[]> GetAllUsers(IEnumerable<UserId> ids, HttpClient client, CancellationToken token)
{
    var fullUserResponseTasks = ids.Select<UserId, Task<HttpResponseMessage>>(id => client.GetAsync($"http://api.url/user/{id.Value}", token));
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

public interface ReadableState<Self, S> : Readable<Self, S>
    where Self : ReadableState<Self, S>, Stateful<Self, S>, Monad<Self>
{
    static K<Self, A> Readable<Self, S>.Asks<A>(Func<S, A> f) => Stateful.gets<Self, S, A>(f);

    static K<Self, A> Readable<Self, S>.Local<A>(Func<S, S> f, K<Self, A> ma) =>
        from initial in Stateful.get<Self, S>()
        from _1 in Stateful.modify<Self, S>(f)
        from a in ma
        from _2 in Stateful.put<Self, S>(initial)
        select a;
}

public interface WritableState<Self, S> : Writable<Self, S>
    where Self : WritableState<Self, S>, Stateful<Self, S>, Monad<Self>
    where S : Monoid<S>
{
    static K<Self, Unit> Writable<Self, S>.Tell(S item) =>
        Stateful.modify<Self, S>(s => s.Combine(item));

    static K<Self, (A Value, S Output)> Writable<Self, S>.Listen<A>(K<Self, A> ma) =>
        from output in Stateful.get<Self, S>()
        from value in ma
        select (value, output);

    static K<Self, A> Writable<Self, S>.Pass<A>(K<Self, (A Value, Func<S, S> Function)> action) =>
        from pair in action
        from _ in Stateful.modify<Self, S>(pair.Function)
        select pair.Value;
}



