using System;
using System.Collections.Generic;
using Core.Scripts.Utils.CustomCollections;

namespace Core.Scripts.Extensions.Linq
{
    public static class EnumerableExtensions
    {
        private static Dictionary<ushort, int> _cachedCounts = new Dictionary<ushort, int>();

        public static List<T> ToList<T>(this IEnumerable<T> source)
        {
            var list = new List<T>();

            foreach (var item in source)
            {
                list.Add(item);
            }

            return list;
        }


        public static ushort GetMostFrequent(this List<ushort> list)
        {
            if (list == null || list.Count == 0)
                throw new ArgumentException("List is empty");

            _cachedCounts.Clear();
            var winner = list[0];
            var maxCount = 0;

            foreach (ushort num in list)
            {
                if (!_cachedCounts.TryAdd(num, 1))
                {
                    _cachedCounts[num]++;
                }

                if (_cachedCounts[num] > maxCount)
                {
                    maxCount = _cachedCounts[num];
                    winner = num;
                }
            }

            return winner;
        }

        public static ushort GetMostFrequent(this FixedUnorderedList<ushort> list)
        {
            if (list == null || list.Count == 0)
                throw new ArgumentException("List is empty");

            _cachedCounts.Clear();
            var winner = list[0];
            var maxCount = 0;

            foreach (ushort num in list.AsSpan())
            {
                if (!_cachedCounts.TryAdd(num, 1))
                {
                    _cachedCounts[num]++;
                }

                if (_cachedCounts[num] > maxCount)
                {
                    maxCount = _cachedCounts[num];
                    winner = num;
                }
            }

            return winner;
        }
        
        public static T Max<T>(this IEnumerable<T> source) where T : IComparable<T>
        {
            using var enumerator = source.GetEnumerator();

            T max = enumerator.Current;

            while (enumerator.MoveNext())
            {
                if (enumerator.Current.CompareTo(max) > 0)
                    max = enumerator.Current;
            }

            return max;
        }

        public static IEnumerable<TResult> Select<TSource, TResult>(
            this IEnumerable<TSource> source, Func<TSource, TResult> selector)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (selector == null)
            {
                throw new ArgumentNullException(nameof(selector));
            }

            return SelectIterator(source, selector);
        }

        private static IEnumerable<TResult> SelectIterator<TSource, TResult>(
            IEnumerable<TSource> source, Func<TSource, TResult> selector)
        {
            foreach (var item in source)
            {
                yield return selector(item);
            }
        }

        public static TSource FirstOrDefault<TSource>(
            this IEnumerable<TSource> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            using (var enumerator = source.GetEnumerator())
            {
                if (enumerator.MoveNext())
                {
                    return enumerator.Current;
                }
            }

            return default(TSource);
        }

        public static TSource FirstOrDefault<TSource>(
            this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            foreach (var element in source)
            {
                if (predicate(element))
                {
                    return element;
                }
            }

            return default(TSource);
        }

        public static bool Contains<T>(this IEnumerable<T> source, T value)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            foreach (T item in source)
            {
                if (EqualityComparer<T>.Default.Equals(item, value))
                {
                    return true;
                }
            }

            return false;
        }

        public static IEnumerable<TResult> SelectMany<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, IEnumerable<TResult>> selector)
        {
            foreach (var item in source)
            {
                foreach (var subItem in selector(item))
                {
                    yield return subItem;
                }
            }
        }

        public static IEnumerable<T> Where<T>(this IEnumerable<T> source, Func<T, bool> predicate)
        {
            if (source == null || predicate == null)
            {
                yield return default;
            }

            foreach (var item in source)
            {
                if (predicate(item))
                {
                    yield return item;
                }
            }
        }

        public static IEnumerable<T> OrderBy<T, TKey>(this IEnumerable<T> source, Func<T, TKey> keySelector)
            where TKey : IComparable<TKey>
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (keySelector == null)
            {
                throw new ArgumentNullException(nameof(keySelector));
            }

            return Sort(source, keySelector, ascending: true);
        }

        public static IEnumerable<T> OrderByDescending<T, TKey>(this IEnumerable<T> source, Func<T, TKey> keySelector)
            where TKey : IComparable<TKey>
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (keySelector == null)
            {
                throw new ArgumentNullException(nameof(keySelector));
            }

            return Sort(source, keySelector, ascending: false);
        }

        private static IEnumerable<T> Sort<T, TKey>(IEnumerable<T> source, Func<T, TKey> keySelector, bool ascending)
            where TKey : IComparable<TKey>
        {
            var sortedList = new List<T>(source);
            sortedList.Sort((x, y) => ascending ? keySelector(x).CompareTo(keySelector(y)) : keySelector(y).CompareTo(keySelector(x)));

            return sortedList;
        }

        public static T Min<T>(this IEnumerable<T> source) where T : IComparable<T>
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            using var enumerator = source.GetEnumerator();

            if (!enumerator.MoveNext())
            {
                throw new InvalidOperationException("Sequence contains no elements");
            }

            T min = enumerator.Current;

            while (enumerator.MoveNext())
            {
                if (enumerator.Current.CompareTo(min) < 0)
                    min = enumerator.Current;
            }

            return min;
        }
    }
}
