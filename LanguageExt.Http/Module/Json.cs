using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using LanguageExt.Common;
using LanguageExt.Traits;
using static LanguageExt.Prelude;

namespace LanguageExt;

//todo separate library
public static class Json<F> where F : Fallible<F>, Applicative<F>
{
    //todo move this + duplicate somewhere else, or a little duplication is perhaps fine?
    private static K<F, A> @try<A>(Func<A> run) => Try.lift(run).Match(F.Pure, F.Fail<A>);

    public static K<F, JsonElement> parse(Stream stream) 
        => @try(() => JsonDocument.Parse(stream).RootElement)
            .Catch(err => new JsonError("Could not parse json stream", err));

    public static K<F, JsonElement> parse(string str) 
        => @try(() => JsonDocument.Parse(str).RootElement)
            .Catch(err => new JsonError($"Could not parse json result {limitLength(str)}", err));

    public static K<F, Result> cast<Result>(JsonElement json)
        => @try<Result>(() =>
            json.Deserialize<Result>() ??
            throw new JsonException($"Could not convert json element {json.ValueKind} to {typeof(Result).Name}"))
        .Catch(err => new JsonError(err.Message, err));
    
    public static K<F, Result> deserialize<Result>(Stream stream)
        => @try<Result>(() => JsonSerializer.Deserialize<Result>(stream) ??
                              throw new JsonException(
                                  $"Could not deserialize json stream result to {typeof(Result).Name}"))
            .Catch(err => new JsonError(err.Message, err));
    
    public static K<F, Result> deserialize<Result>(string str)
        => @try<Result>(() =>
            JsonSerializer.Deserialize<Result>(str) ??
            throw new JsonException($"Could not deserialize json result {limitLength(str)} to {typeof(Result).Name}"))
        .Catch(err => new JsonError(err.Message, err));

    
    public static Func<JsonElement, K<F, JsonElement>> key(string objKey) => json => key(objKey, json);

    public static K<F, JsonElement> key(string key, JsonElement json) =>
        @try(() => json.GetProperty(key))
            .Catch(err => new JsonError(keyErrorMessage(key, json), err));

    private static string keyErrorMessage(string key, JsonElement json) =>
        $"Unable to lookup key '{key}' in {json.ValueKind}: " + (json.ValueKind == JsonValueKind.Object
            ? $"object contains keys: [{string.Join(", ", json.EnumerateObject().Select(kv => kv.Name))}]"
            : shortToString(json));

    public static Option<JsonElement> safeKey(string key, JsonElement json) =>
        Try.lift(() => json.GetProperty(key)).ToOption();

    public static K<F, Seq<JsonElement>> iterate(JsonElement json) =>
        @try(json.EnumerateArray).Map(array => toSeq(array))
            .Catch(err => new JsonError($"Unable to enumerate {json.ValueKind}: {shortToString(json)}"));

    public static Func<JsonElement, K<F, JsonElement>> index(Index idx) => json => index(idx, json);
    
    public static K<F, JsonElement> index(Index idx, JsonElement json) =>
        @try(() => json[idx.Value]).Catch(err => new JsonError(indexErrorMessage(idx, json), err));

    private static string indexErrorMessage(Index idx, JsonElement json) =>
        $"Unable to lookup index {idx.Value} in {json.ValueKind}: " + (json.ValueKind == JsonValueKind.Array
            ? $"array is length {json.EnumerateArray().Count()}"
            : shortToString(json));

    private static string shortToString(JsonElement json) => limitLength(json.ToString());

    private static string limitLength(string str) => 
        str.Length < 100 ? str : str.Substring(0, 100) + "...";
}
public record JsonError(string Message, Option<Error> Inner = default) : Expected(Message, Code, Inner)
{
    public const int Code = 7654;
}

[Obsolete("Hopefully will be in main lib soon https://github.com/louthy/language-ext/pull/1539")]
public static class Kleisli
{
    extension<M, A, B, C>(Func<A, K<M, B>> self) where M : Monad<M>
    {
        /// <summary>
        /// Equivalent to Haskell's `>=>` or "fish" operator
        /// </summary>
        public static Func<A, K<M, C>> operator >> (Func<A, K<M, B>> bind1, Func<B, K<M, C>> bind2) =>
            a => bind1(a).Bind(bind2);
    }
}
