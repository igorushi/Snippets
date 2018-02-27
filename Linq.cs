using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace System.Linq
{
    static class ExtensionMethods
    {
        public static void AddMany<T>(this IList<T> list, IEnumerable<T> elements)
        {
            foreach (var item in elements)
            {
                list.Add(item);
            }
        }

        public static IEnumerable<T> Except<T>(this IEnumerable<T> enumerable, T element)
        {
            return enumerable.Except(new[] { element });
        }
        public static IEnumerable<T> And<T>(this IEnumerable<T> enumerable, T element)
        {
            return enumerable.Concat(new[] { element });
        }

        public static IEnumerable<T> And<T>(this T element, IEnumerable<T> enumerable)
        {
            return new[] { element }.Concat(enumerable);
        }

        public static T[] ToArray<T>(this IEnumerable enumerable)
        {
            return enumerable.Cast<T>().ToArray();
        }

        public static IEnumerable<T> Repeat<T>(this IEnumerable<T> enumerable, int times)
        {
            return Enumerable.Range(0, times).SelectMany(i => enumerable);
        }

        public static IEnumerable<IReadOnlyList<T>> Batch<T>(this IEnumerable<T> collection, int batchSize)
        {
            List<T> nextbatch = new List<T>(batchSize);
            foreach (T item in collection)
            {
                nextbatch.Add(item);
                if (nextbatch.Count == batchSize)
                {
                    yield return nextbatch;
                    nextbatch = new List<T>(batchSize);
                }
            }
            if (nextbatch.Count > 0)
                yield return nextbatch;
        }

        public static IEnumerable<T> WithProgressReporting<T>(this IEnumerable<T> sequence, Action<int> reportProgress)
        {
            if (sequence == null)
                throw new ArgumentNullException("sequence");

            var collection = sequence as ICollection<T>;
            if (collection == null)
                collection = new List<T>(sequence);

            int total = collection.Count;
            return collection.WithProgressReporting(total, reportProgress);
        }

        public static IEnumerable<T> WithProgressReporting<T>(this IEnumerable<T> sequence, long itemCount, Action<int> reportProgress)
        {
            if (sequence == null)
                throw new ArgumentNullException("sequence");

            int completed = 0;
            int progress = 0;
            foreach (var item in sequence)
            {
                yield return item;
                completed++;
                int newProgress = (int)(((double)completed / itemCount) * 100);
                if (newProgress > progress)
                    reportProgress((progress = newProgress));
            }
        }

        public static IEnumerable<TResult> GroupBy<TSource, TKey, TResult>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TKey, IEnumerable<TSource>, TResult> resultSelector, Func<TKey, TKey, bool> lambdaComparer)
        {
            return source.GroupBy(keySelector, resultSelector, new LambdaComparer<TKey>(lambdaComparer));
        }

        public static IEnumerable<T> Distinct<T>(this IEnumerable<T> collection, Func<T, T, bool> lambdaComparer)
        {
            return collection.Distinct(new LambdaComparer<T>(lambdaComparer));
        }

        public static T OneBeforeLast<T>(this IEnumerable<T> enumerable)
        {
            return enumerable.ElementAt(enumerable.Count() - 2);
        }

        public static T OneBeforeLastOrDefault<T>(this IEnumerable<T> enumerable)
        {
            return enumerable.ElementAtOrDefault(enumerable.Count() - 2);
        }

        public static T Second<T>(this IEnumerable<T> enumerable)
        {
            return enumerable.ElementAt(1);
        }

        public static T SecondOrDefault<T>(this IEnumerable<T> enumerable)
        {
            return enumerable.ElementAtOrDefault(1);
        }
    }


    //from http://brendan.enrick.com/post/LINQ-Your-Collections-with-IEqualityComparer-and-Lambda-Expressions
    class LambdaComparer<T> : IEqualityComparer<T>
    {
        private readonly Func<T, T, bool> _lambdaComparer;
        private readonly Func<T, int> _lambdaHash;

        public LambdaComparer(Func<T, T, bool> lambdaComparer) :
            this(lambdaComparer, o => 0)
        {
        }

        public LambdaComparer(Func<T, T, bool> lambdaComparer, Func<T, int> lambdaHash)
        {
            _lambdaComparer = lambdaComparer ?? throw new ArgumentNullException(nameof(lambdaComparer));
            _lambdaHash = lambdaHash ?? throw new ArgumentNullException(nameof(lambdaHash));
        }

        public bool Equals(T x, T y)
        {
            return _lambdaComparer(x, y);
        }

        public int GetHashCode(T obj)
        {
            return _lambdaHash(obj);
        }
    }
}
