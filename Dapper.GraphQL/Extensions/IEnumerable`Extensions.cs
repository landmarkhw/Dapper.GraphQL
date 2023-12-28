using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dapper.GraphQL
{
    public static class EnumerableExtensions
    {
        /// <summary>
        /// An async version of the Aggregate with seed method found in dotnet/corefx
        /// https://github.com/dotnet/corefx/blob/master/src/System.Linq/src/System/Linq/Aggregate.cs
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TAccumulate"></typeparam>
        /// <param name="source"></param>
        /// <param name="seed"></param>
        /// <param name="funcAsync"></param>
        /// <returns></returns>
        public static async Task<TAccumulate> AggregateAsync<TSource, TAccumulate>(this IEnumerable<TSource> source, TAccumulate seed, Func<TAccumulate, TSource, Task<TAccumulate>> funcAsync)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (funcAsync == null)
            {
                throw new ArgumentNullException(nameof(funcAsync));
            }

            var result = seed;
            foreach (var element in source)
            {
                result = await funcAsync(result, element);
            }

            return result;
        }
    }
}
