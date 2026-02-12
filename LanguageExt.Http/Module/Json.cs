using System.Text.Json;
using LanguageExt.Common;
using LanguageExt.Traits;
using static LanguageExt.Prelude;

namespace LanguageExt;

//todo separate library
public static  class Json
{
    //todo move this + duplicate somewhere else, or a little duplication is perhaps fine?
    private static K<M, A> @try<M, A>(Func<A> run) where M : Applicative<M>, Fallible<M>
        => Try.lift(run).Match(M.Pure, M.Fail<A>);

    public static K<F, JsonElement> parse<F>(Stream stream) 
        where F : Fallible<F>, Applicative<F> 
        => @try<F, JsonElement>(() => JsonDocument.Parse(stream).RootElement);

    public static K<F, JsonElement> parse<F>(String str) 
        where F : Fallible<F>, Applicative<F> 
        =>
            @try<F, JsonElement>(() => JsonDocument.Parse(str).RootElement);

    public static K<F, Result> deserialize<F, Result>(JsonElement json)
        where F : Fallible<F>, Monad<F>
        =>  from resultNull in @try<F, Result>(() => json.Deserialize<Result>())
            from result in Prelude.Optional<Result>(resultNull).Match(F.Pure<Result>, 
                () => F.Fail<Result>(Error.New($"Could not convert json element {json.ValueKind} to {typeof(Result).Name}")))
            select result;

    public static K<F, Result> deserialize<F, Result>(Stream stream)
        where F : Fallible<F>, Monad<F>
        =>
            from resultNull in @try<F, Result>(() => JsonSerializer.Deserialize<Result>(stream))
            from result in Prelude.Optional<Result>(resultNull).Match(F.Pure<Result>, 
                () => F.Fail<Result>(Error.New($"Could not deserialize json stream result to {typeof(Result).Name}")))
            select result;

    public static K<F, Result> deserialize<F, Result>(string str)
        where F : Fallible<F>, Monad<F>
        =>
            from resultNull in @try<F, Result>(() => JsonSerializer.Deserialize<Result>(str))
            from result in Prelude.Optional<Result>(resultNull).Match(F.Pure<Result>, 
                () => F.Fail<Result>(Error.New($"Could not deserialize json result {str} to {typeof(Result).Name}")))
            select result;

    public static Func<JsonElement, K<F, JsonElement>> key<F>(string key)
        where F : Fallible<F>, Applicative<F> => json => key<F>(key, json);

    public static K<F, JsonElement> key<F>(string key, JsonElement json)
        where F : Fallible<F>, Applicative<F> =>
        //todo more helpful error message
        @try<F, JsonElement>(() => json.GetProperty(key));

    public static K<F, Seq<JsonElement>> iterate<F>(JsonElement json)
        where F : Fallible<F>, Applicative<F> =>
        //todo more helpful error message
        @try<F, JsonElement.ArrayEnumerator>(json.EnumerateArray)
            .Map(array => toSeq(array));
}