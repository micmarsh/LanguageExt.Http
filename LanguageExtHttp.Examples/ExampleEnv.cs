using LanguageExt;

namespace LanguageExtHttp.Examples;

/// <summary>
/// A more "realistic" example of an environment/runtime an application would use
/// </summary>
/// <param name="Client">The HttpClient, needed for any testing to be relevant!</param>
/// <param name="Session">Simulation of some kind of user session or authentication: doesn't strictly need to be wrapped in IO in
/// examples but probably would be in a real scenario</param>
/// <param name="Now">Get the current date, obviously an impure operation</param>
public record ExampleEnv(HttpClient Client, IO<string> Session, IO<DateTime> Now) : HasHttpClient;