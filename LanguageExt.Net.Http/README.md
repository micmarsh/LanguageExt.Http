# LanguageExt HTTP

A functional wrapper around HttpClient intended to integrate into LangagueExt V5 based workflows.

## Rationale

Provides the expected methods (`get`, `post`, `delete`, etc.) returning a `Http<HttpResponseMessage>`, an "Http Monad"

This monad is a wrapper around `ReaderT<HttpEnv, IO>`, (and `HttpEnv` is for most practical purposes a wrapper around `HttpClient`) meaning we get a few benefits from this approach
* Lazy execution
* Ability to utilize all of the `UnliftMonadIO` goodies: `Retry`, `Repeat`, `Fork`, etc.
* Enables threading of `HttpClient` instances in a "reader monad style"
  * More readable code (no passing `HttpClient` instances down call stacks)
  * Enable best-practice `HttpClient` instance management from caller depending on application (socket exhaustion issue)
  * Easier testing (see `client` helper methods as well)

## Usage


## TODO fill out this README and supporting docs