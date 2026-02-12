using LanguageExt.Traits;

namespace LanguageExt;

//todo contribute to main lib?
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