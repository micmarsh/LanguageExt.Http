# LanguageExt HTTP

A functional wrapper around HttpClient intended to integrate into LangagueExt V5 based workflows.

## Rationale

Provides the expected methods (`get`, `post`, `delete`, etc.) returning a `Http<HttpResponseMessage>`, an "Http Monad"

#### TODO AXE THIS SECTION FOR A CODE EXAMPLE, IMPERATIVE VS. THIS, THAT EXERCISES ALL POSSIBLITIES?
This monad is a `Fallible` wrapper around `ReaderT<HttpEnv, IO>`,  (and `HttpEnv` is just a wrapper around `HttpClient`) meaning we get a few benefits from this approach
* Ability to utilize modular and generic error handling with `Fallible`
* Lazy execution
* Ability to utilize all of the `UnliftMonadIO` goodies: `Retry`, `Repeat`, `Fork`, etc.
* Enables threading of `HttpClient` instances in a "reader monad style"
  * More readable code (no passing `HttpClient` instances down call stacks)
  * Allows best-practice `HttpClient` instance management from caller depending on application (socket exhaustion issue)
  * Easier testing (see `client` helper methods as well)

## Usage


## TODO fill out this README and supporting docs