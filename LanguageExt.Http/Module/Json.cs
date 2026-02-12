using System.Text.Json;
using LanguageExt.Common;
using LanguageExt.Traits;
using static LanguageExt.Prelude;

namespace LanguageExt;

//todo separate library
// todo move generic to class type argument, delete concrete helpers, encourage static import "module style"!
 // maybe this for Http generic methods too? Let's not get too far ahead of ourselves, this^ needs done if you'll ever release at all
public static class Json<F> where F : Fallible<F>, Monad<F>
{
    //todo move this + duplicate somewhere else, or a little duplication is perhaps fine?
    private static K<F, A> @try<A>(Func<A> run) 
        => Try.lift(run).Match(F.Pure, F.Fail<A>);

    public static K<F, JsonElement> parse(Stream stream) 
        => @try(() => JsonDocument.Parse(stream).RootElement);

    public static K<F, JsonElement> parse(String str) 
        => @try(() => JsonDocument.Parse(str).RootElement);

    public static K<F, Result> cast<Result>(JsonElement json)
        =>  from resultNull in @try<Result>(() => json.Deserialize<Result>())
            from result in Optional<Result>(resultNull).Match(F.Pure, 
                () => F.Fail<Result>(Error.New($"Could not convert json element {json.ValueKind} to {typeof(Result).Name}")))
            select result;

    public static K<F, Result> deserialize<Result>(Stream stream)
        =>
            from resultNull in @try<Result>(() => JsonSerializer.Deserialize<Result>(stream))
            from result in Optional<Result>(resultNull).Match(F.Pure, 
                () => F.Fail<Result>(Error.New($"Could not deserialize json stream result to {typeof(Result).Name}")))
            select result;

    public static K<F, Result> deserialize<Result>(string str)
        =>
            from resultNull in @try<Result>(() => JsonSerializer.Deserialize<Result>(str))
            from result in Optional<Result>(resultNull).Match(F.Pure, 
                () => F.Fail<Result>(Error.New($"Could not deserialize json result {str} to {typeof(Result).Name}")))
            select result;

    public static Func<JsonElement, K<F, JsonElement>> key(string objKey) => json => key(objKey, json);

    public static K<F, JsonElement> key(string key, JsonElement json) =>
        //todo more helpful error message
        @try(() => json.GetProperty(key));

    public static K<F, Seq<JsonElement>> iterate(JsonElement json) =>
        //todo more helpful error message
        @try(json.EnumerateArray).Map(array => toSeq(array));

    public static Func<JsonElement, K<F, JsonElement>> index(Index idx) => json => index(idx, json);
    
    public static K<F, JsonElement> index(Index idx, JsonElement json) =>
        //todo more helpful error message
        @try(() => json[idx.Value]);
}