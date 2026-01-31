using System.Runtime.CompilerServices;
using LanguageExt.Traits;

namespace LanguageExt;

public static class HttpExt
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Http<A> As<A>(this K<Http, A> ma) => (Http<A>)ma;

    public static IO<A> Run<A>(this K<Http, A> ma, Option<HttpClient> client = default) => ma.As().Run(client);

    public static Http<C> SelectMany<A, B, C>(this K<Http, A> ma, Func<A, Http<B>> bind, Func<A, B, C> project) =>
        ma.As().SelectMany(bind, project);
    
    public static Http<C> SelectMany<A, B, C>(this Ask<HttpEnv, A> ask, Func<A, Http<B>> bind, Func<A, B, C> project) =>
        ((Http<A>)ask).SelectMany(bind, project);
    
    extension<A, B>(K<Http, A> self)
    {
        /// <summary>
        /// Monad bind operator
        /// </summary>
        /// <param name="ma">Monad to bind</param>
        /// <param name="f">Binding function</param>
        /// <returns>Mapped monad</returns>
        public static Http<B> operator >> (K<Http, A> ma, Func<A, K<Http, B>> f) =>
            +ma.Bind(f);
        
        /// <summary>
        /// Sequentially compose two actions, discarding any value produced by the first, like sequencing operators (such
        /// as the semicolon) in C#.
        /// </summary>
        /// <param name="lhs">First action to run</param>
        /// <param name="rhs">Second action to run</param>
        /// <returns>Result of the second action</returns>
        public static Http<B> operator >> (K<Http, A> lhs, K<Http, B> rhs) =>
            lhs >> (_ => rhs);
    }
    
    extension<A>(K<Http, A> self)
    {
        /// <summary>
        /// Sequentially compose two actions.  The second action is a unit-returning action, so the result of the
        /// first action is propagated. 
        /// </summary>
        /// <param name="lhs">First action to run</param>
        /// <param name="rhs">Second action to run</param>
        /// <returns>Result of the first action</returns>
        public static Http<A> operator >> (K<Http, A> lhs, K<Http, Unit> rhs) =>
            lhs >> (x => (_ => x) * rhs);
        
        /// <summary>
        /// Downcast operator
        /// </summary>
        public static Http<A> operator +(K<Http, A> ma) =>
            (Http<A>)ma;
        
        /// <summary>
        /// Downcast operator
        /// </summary>
        public static Http<A> operator >> (K<Http, A> ma, Lower lower) =>
            +ma;
    }
}