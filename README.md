# LanguageExt HTTP

A functional wrapper around HttpClient intended to integrate into LangagueExt V5 based workflows.

Provides the expected methods (`get`, `post`, `delete`, etc.) returning `Http<HttpResponseMessage>`, an "Http Monad"
## Rationale
If you're already convinced of the general preferability of the functional approach, you probably don't need this `Rationale` section.

If you're not convinced but curious, check out the [code cleanup project](https://github.com/micmarsh/LanguageExt.Http/tree/master/LanguageExt.Net.Http.CodeCleanupExample), noting the differences between the functional and imperative approaches, not just in total lines of code, but also the greater simplicity[^1] of nearly every part of the functional approach.

I may create a more dedicated "literate coding" style writeup of the above in the future (as of 10/23/2025), but for now hopefully the code can speak for iself.

[^1] Simplicity in the [Rich Hickey sense of the word](https://www.youtube.com/watch?v=SxdOUGdseq4), it may not be _easy_ at first if you're not familiar with the concepts!

## Usage
Add `LanguageExt.Net.Http 0.1.0-alpha-3` on nuget.

```csharp
// add to GlobalUsings as appropriate
using LanguageExt.Net;
using static LanguageExt.Net.Http;
```
The `Http` monad implements and thus gives us
* `Monad`, for basic sequencing and composition, `Bind` (with LINQ syntax) and `Traverse` being the bread and butter of most of what you'll do
* `Fallible`, for generalizable and modular error handling
* `MonadUnliftIO` for not only lifting arbitrary IO operations (such as debuggings), but also access to `Retry`, `Fork`, `Repeat` and related goodes
* `Readable`, to enable threading of an `HttpClient` throughout the application
  * If you need to thread `CancellationToken` as well, you can utilize `IO`'s built-in `EnvIO`

You can refer to [the examples](https://github.com/micmarsh/LanguageExt.Http/blob/master/LanguageExt.Net.Http.CodeCleanupExample/Program.cs#L102) to see most of the above in action.

However, since a concrete `Http` type is an obstacle to composition in large applications, nearly every method in this library has both an `Http`-based and generalized version, for exmaple
* "The basics" (`get`, `post`, `delete`, etc.), can be generalized to any `MonadIO` that implements `Readable` for an `Env` that implements this library's `HasHttpClient` interface
* `parseUri` can be generalized to any `Fallible` `Applicative`
* Response parsing methods such as `readContentAsStream` can be generalized to any `MonadIO`

### Testing
Mocking `HttpClient`s is a thorn in many peoples sides, so this library provides a `Http.client` method that, given a `Func<HttpResponseMessage, HttpResponseMessage>` ( [or other overload](https://github.com/micmarsh/LanguageExt.Http/blob/master/LanguageExt.Net.Http/Module/Http.Module.Client.cs) ) handles all of the nasty business of dealing with an `HttpMessageHandler` for you.

`client` combined with the natural structure of the "reader monad pattern" this follows should enable much smoother mocking of http functionality in general.

Copyright 2025 Michael Marsh