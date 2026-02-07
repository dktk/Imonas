using SG.Common;
using System.Runtime.CompilerServices;

namespace System
{
    public static class Guard
    {
        public static Result<string> AgainstNullOrEmpty(this string value, string? message = default, [CallerMemberName] string memberName = "", [CallerArgumentExpression("value")] string argumentExpression = null)
        {
            return value.IsNullOrWhiteSpace() ? Result<string>.BuildFailure($"{value}") : Result<string>.BuildSuccess(value);
        }

        public static Result<int> AgainstZeroAndNegatives(this int value, string? message = default, [CallerMemberName] string memberName = "", [CallerArgumentExpression("value")] string argumentExpression = null)
        {
            return value > 0 ? Result<int>.BuildSuccess(value) : Result<int>.BuildFailure($"{value}");
        }

        public static Result<long> AgainstZeroAndNegatives(this long value, string? message = default, [CallerMemberName] string memberName = "", [CallerArgumentExpression("value")] string argumentExpression = null)
        {
            return value > 0 ? Result<long>.BuildSuccess(value) : Result<long>.BuildFailure($"{value}");
        }

        /// <summary>
        ///     Checks if an argument is null. If it is, throws an <see cref="ArgumentNullException" /> with the specified
        ///     <paramref name="argName" />
        /// </summary>
        /// <typeparam name="T">type of the argument, must be a reference type</typeparam>
        /// <param name="arg">argument to check</param>
        /// <param name="argName">name of argument</param>
        /// <exception cref="ArgumentNullException"></exception>
        public static void ArgNotNull<T>(T arg, string argName) where T : class
        {
            if (ReferenceEquals(arg, null))
            {
                throw new ArgumentNullException(argName);
            }
        }

    }
}
