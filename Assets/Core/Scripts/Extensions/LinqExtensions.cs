using System;
using System.Collections;
using System.Collections.Generic;

namespace Core.Scripts.Extensions
{
    /// <summary>Various LinQ extensions.</summary>
    /// <footer><a href="https://www.google.com/search?q=Sirenix.Utilities.LinqExtensions">`LinqExtensions` on google.com</a></footer>
    public static partial class LinqExtensions
    {
        public static bool Any<T>(this T[] source)
        {
            return source.Length > 0;
        }

        public static bool Any<T>(this T[] source, Func<T, bool> predicate)
        {
            for (var i = 0; i < source.Length; i++)
            {
                if (predicate(source[i]))
                {
                    return true;
                }
            }
            return false;
        }
        
        public static bool All<T>(this T[] source, Func<T, bool> predicate)
        {
            for (var i = 0; i < source.Length; i++)
            {
                if (!predicate(source[i]))
                {
                    return false;
                }
            }
            return true;
        }
        
        public static int Count<T>(T[] source, Func<T, bool> predicate)
        {
            var count = 0;
            for (var i = 0; i < source.Length; i++)
            {
                if (predicate(source[i]))
                {
                    count++;
                }
            }
            return count;
        }
        
        public static T FirstOrDefault<T>(this T[] source, Func<T, bool> predicate)
        {
            for (var i = 0; i < source.Length; i++)
            {
                if (predicate(source[i]))
                {
                    return source[i];
                }
            }
            return default;
        }
        
        
        private static void QuickSort<T, TKey>(T[] arr, Func<T, TKey> keySelector, int low, int high) where TKey : IComparable<TKey>
        {
            if (low >= high)
            {
                return;
            }
            var pi = Partition(arr, keySelector, low, high);
            QuickSort(arr, keySelector, low, pi - 1);
            QuickSort(arr, keySelector, pi + 1, high);
        }

        private static int Partition<T, TKey>(T[] arr, Func<T, TKey> keySelector, int low, int high) where TKey : IComparable<TKey>
        {
            var pivot = keySelector(arr[high]);
            var i = (low - 1);
            for (var j = low; j <= high - 1; j++)
            {
                if (keySelector(arr[j]).CompareTo(pivot) >= 0)
                {
                    continue;
                }
                
                i++;
                (arr[i], arr[j]) = (arr[j], arr[i]);
            }
            (arr[i + 1], arr[high]) = (arr[high], arr[i + 1]);
            return i + 1;
        }
        
        public static T[] OrderBy<T, TKey>(this T[] source, Func<T, TKey> keySelector) where TKey : IComparable<TKey>
        { 
            var arr = (T[])source.Clone();
            QuickSort(arr, keySelector, 0, arr.Length - 1);
            return arr;
        }

        
        public static List<T> ToList<T>(this T[] source)
        {
            var result = new List<T>(source.Length);

            for (var i = 0; i < source.Length; i++)
            {
                result.Add(source[i]);
            }

            return result;
        }

        public static TResult[] Select<TSource, TResult>(this TSource[] source, Func<TSource, TResult> selector)
        {
            var result = new TResult[source.Length];
        
            for (var i = 0; i < source.Length; i++)
            {
                result[i] = selector(source[i]);
            }
        
            return result;
        }
        
        public static TResult[] Select<TSource, TResult>(
            this TSource[] source,
            Func<TSource, int, TResult> selector)
        {
            var result = new TResult[source.Length];

            for (var i = 0; i < source.Length; i++)
            {
                result[i] = selector(source[i], i);
            }

            return result;
        }
        
        public static T[] Where<T>(this T[] source, Func<T, bool> predicate)
        {
            var count = 0;

            for (var i = 0; i < source.Length; i++)
            {
                if (predicate(source[i]))
                {
                    count++;
                }
            }

            var result = new T[count];
            var index = 0;

            for (var i = 0; i < source.Length; i++)
            {
                if (predicate(source[i]))
                {
                    result[index] = source[i];
                    index++;
                }
            }

            return result;
        }
        
        public static bool Contains<T>(this T[] source, T target)
        {
            for (var i = 0; i < source.Length; i++)
            {
                if (EqualityComparer<T>.Default.Equals(source[i], target))
                {
                    return true;
                }
            }

            return false;
        }

        public static int IndexOf<T>(this T[] source, Func<T, bool> condition)
        {
            for (var i = 0; i < source.Length; i++)
            {
                if (condition(source[i]))
                {
                    return i;
                }
            }
            
            return -1;
        }
        
        public static int[] IndicesWhere<T>(this T[] source, Func<T, bool> predicate)
        {
            var count = 0;

            for (var i = 0; i < source.Length; i++)
            {
                if (predicate(source[i]))
                {
                    count++;
                }
            }

            var result = new int[count];
            var index = 0;

            for (var i = 0; i < source.Length; i++)
            {
                if (!predicate(source[i]))
                {
                    continue;
                }

                result[index] = i;
                index++;
            }

            return result;
        }
        
        /// <summary>Calls an action on each item before yielding them.</summary>
        /// <param name="source">The collection.</param>
        /// <param name="action">The action to call for each item.</param>
        /// <footer><a href="https://www.google.com/search?q=Sirenix.Utilities.LinqExtensions.Examine">`LinqExtensions.Examine` on google.com</a></footer>
        public static IEnumerable<T> Examine<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (var obj in source)
            {
                action(obj);
                yield return obj;
            }
        }

        /// <summary>Perform an action on each item.</summary>
        /// <param name="source">The source.</param>
        /// <param name="action">The action to perform.</param>
        /// <footer><a href="https://www.google.com/search?q=Sirenix.Utilities.LinqExtensions.ForEach">`LinqExtensions.ForEach` on google.com</a></footer>
        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (var obj in source)
            {
                action(obj);
            }

            return source;
        }

        /// <summary>Perform an action on each item.</summary>
        /// <param name="source">The source.</param>
        /// <param name="action">The action to perform.</param>
        /// <footer><a href="https://www.google.com/search?q=Sirenix.Utilities.LinqExtensions.ForEach">`LinqExtensions.ForEach` on google.com</a></footer>
        public static IEnumerable<T> ForEach<T>(
            this IEnumerable<T> source,
            Action<T, int> action)
        {
            var num = 0;

            foreach (var obj in source)
            {
                action(obj, num++);
            }

            return source;
        }

        /// <summary>Convert each item in the collection.</summary>
        /// <param name="source">The collection.</param>
        /// <param name="converter">Func to convert the items.</param>
        /// <footer><a href="https://www.google.com/search?q=Sirenix.Utilities.LinqExtensions.Convert">`LinqExtensions.Convert` on google.com</a></footer>
        public static IEnumerable<T> Convert<T>(
            this IEnumerable source,
            Func<object, T> converter)
        {
            foreach (var obj in source)
            {
                yield return converter(obj);
            }
        }

        /// <summary>Convert a colletion to a HashSet.</summary>
        /// <footer><a href="https://www.google.com/search?q=Sirenix.Utilities.LinqExtensions.ToHashSet">`LinqExtensions.ToHashSet` on google.com</a></footer>
        [Obsolete("Just write new HashSet<T>(source) instead - this extension is causing naming conflict issues in Unity 2021.2+ and will be removed in the future.", false)]
        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> source)
        {
            return new(source);
        }

        /// <summary>Convert a colletion to a HashSet.</summary>
        /// <footer><a href="https://www.google.com/search?q=Sirenix.Utilities.LinqExtensions.ToHashSet">`LinqExtensions.ToHashSet` on google.com</a></footer>
        [Obsolete("Just write new HashSet<T>(source, comparer) instead - this extension is causing naming conflict issues in Unity 2021.2+ and will be removed in the future.",
            false)]
        public static HashSet<T> ToHashSet<T>(
            this IEnumerable<T> source,
            IEqualityComparer<T> comparer)
        {
            return new HashSet<T>(source, comparer);
        }

        /// <summary>Add an item to the beginning of a collection.</summary>
        /// <param name="source">The collection.</param>
        /// <param name="prepend">Func to create the item to prepend.</param>
        /// <footer><a href="https://www.google.com/search?q=Sirenix.Utilities.LinqExtensions.PrependWith">`LinqExtensions.PrependWith` on google.com</a></footer>
        public static IEnumerable<T> PrependWith<T>(
            this IEnumerable<T> source,
            Func<T> prepend)
        {
            yield return prepend();

            foreach (var obj in source)
            {
                yield return obj;
            }
        }

        /// <summary>Add an item to the beginning of a collection.</summary>
        /// <param name="source">The collection.</param>
        /// <param name="prepend">The item to prepend.</param>
        /// <footer><a href="https://www.google.com/search?q=Sirenix.Utilities.LinqExtensions.PrependWith">`LinqExtensions.PrependWith` on google.com</a></footer>
        public static IEnumerable<T> PrependWith<T>(this IEnumerable<T> source, T prepend)
        {
            yield return prepend;

            foreach (var obj in source)
            {
                yield return obj;
            }
        }

        /// <summary>
        /// Add a collection to the beginning of another collection.
        /// </summary>
        /// <param name="source">The collection.</param>
        /// <param name="prepend">The collection to prepend.</param>
        /// <footer><a href="https://www.google.com/search?q=Sirenix.Utilities.LinqExtensions.PrependWith">`LinqExtensions.PrependWith` on google.com</a></footer>
        public static IEnumerable<T> PrependWith<T>(
            this IEnumerable<T> source,
            IEnumerable<T> prepend)
        {
            foreach (var obj in prepend)
            {
                yield return obj;
            }

            foreach (var obj in source)
            {
                yield return obj;
            }
        }

        /// <summary>
        /// Add an item to the beginning of another collection, if a condition is met.
        /// </summary>
        /// <param name="source">The collection.</param>
        /// <param name="condition">The condition.</param>
        /// <param name="prepend">Func to create the item to prepend.</param>
        /// <footer><a href="https://www.google.com/search?q=Sirenix.Utilities.LinqExtensions.PrependIf">`LinqExtensions.PrependIf` on google.com</a></footer>
        public static IEnumerable<T> PrependIf<T>(
            this IEnumerable<T> source,
            bool condition,
            Func<T> prepend)
        {
            if (condition)
            {
                yield return prepend();
            }

            foreach (var obj in source)
            {
                yield return obj;
            }
        }

        /// <summary>
        /// Add an item to the beginning of another collection, if a condition is met.
        /// </summary>
        /// <param name="source">The collection.</param>
        /// <param name="condition">The condition.</param>
        /// <param name="prepend">The item to prepend.</param>
        /// <footer><a href="https://www.google.com/search?q=Sirenix.Utilities.LinqExtensions.PrependIf">`LinqExtensions.PrependIf` on google.com</a></footer>
        public static IEnumerable<T> PrependIf<T>(
            this IEnumerable<T> source,
            bool condition,
            T prepend)
        {
            if (condition)
            {
                yield return prepend;
            }

            foreach (var obj in source)
            {
                yield return obj;
            }
        }

        /// <summary>
        /// Add a collection to the beginning of another collection, if a condition is met.
        /// </summary>
        /// <param name="source">The collection.</param>
        /// <param name="condition">The condition.</param>
        /// <param name="prepend">The collection to prepend.</param>
        /// <footer><a href="https://www.google.com/search?q=Sirenix.Utilities.LinqExtensions.PrependIf">`LinqExtensions.PrependIf` on google.com</a></footer>
        public static IEnumerable<T> PrependIf<T>(
            this IEnumerable<T> source,
            bool condition,
            IEnumerable<T> prepend)
        {
            if (condition)
            {
                foreach (var obj in prepend)
                {
                    yield return obj;
                }
            }

            foreach (var obj in source)
            {
                yield return obj;
            }
        }

        /// <summary>
        /// Add an item to the beginning of another collection, if a condition is met.
        /// </summary>
        /// <param name="source">The collection.</param>
        /// <param name="condition">The condition.</param>
        /// <param name="prepend">Func to create the item to prepend.</param>
        /// <footer><a href="https://www.google.com/search?q=Sirenix.Utilities.LinqExtensions.PrependIf">`LinqExtensions.PrependIf` on google.com</a></footer>
        public static IEnumerable<T> PrependIf<T>(
            this IEnumerable<T> source,
            Func<bool> condition,
            Func<T> prepend)
        {
            if (condition())
            {
                yield return prepend();
            }

            foreach (var obj in source)
            {
                yield return obj;
            }
        }

        /// <summary>
        /// Add an item to the beginning of another collection, if a condition is met.
        /// </summary>
        /// <param name="source">The collection.</param>
        /// <param name="condition">The condition.</param>
        /// <param name="prepend">The item to prepend.</param>
        /// <footer><a href="https://www.google.com/search?q=Sirenix.Utilities.LinqExtensions.PrependIf">`LinqExtensions.PrependIf` on google.com</a></footer>
        public static IEnumerable<T> PrependIf<T>(
            this IEnumerable<T> source,
            Func<bool> condition,
            T prepend)
        {
            if (condition())
            {
                yield return prepend;
            }

            foreach (var obj in source)
            {
                yield return obj;
            }
        }

        /// <summary>
        /// Add a collection to the beginning of another collection, if a condition is met.
        /// </summary>
        /// <param name="source">The collection.</param>
        /// <param name="condition">The condition.</param>
        /// <param name="prepend">The collection to prepend.</param>
        /// <footer><a href="https://www.google.com/search?q=Sirenix.Utilities.LinqExtensions.PrependIf">`LinqExtensions.PrependIf` on google.com</a></footer>
        public static IEnumerable<T> PrependIf<T>(
            this IEnumerable<T> source,
            Func<bool> condition,
            IEnumerable<T> prepend)
        {
            if (condition())
            {
                foreach (var obj in prepend)
                {
                    yield return obj;
                }
            }

            foreach (var obj in source)
            {
                yield return obj;
            }
        }

        /// <summary>
        /// Add an item to the beginning of another collection, if a condition is met.
        /// </summary>
        /// <param name="source">The collection.</param>
        /// <param name="condition">The condition.</param>
        /// <param name="prepend">Func to create the item to prepend.</param>
        /// <footer><a href="https://www.google.com/search?q=Sirenix.Utilities.LinqExtensions.PrependIf">`LinqExtensions.PrependIf` on google.com</a></footer>
        public static IEnumerable<T> PrependIf<T>(
            this IEnumerable<T> source,
            Func<IEnumerable<T>, bool> condition,
            Func<T> prepend)
        {
            if (condition(source))
            {
                yield return prepend();
            }

            foreach (var obj in source)
            {
                yield return obj;
            }
        }

        /// <summary>
        /// Add an item to the beginning of another collection, if a condition is met.
        /// </summary>
        /// <param name="source">The collection.</param>
        /// <param name="condition">The condition.</param>
        /// <param name="prepend">The item to prepend.</param>
        /// <footer><a href="https://www.google.com/search?q=Sirenix.Utilities.LinqExtensions.PrependIf">`LinqExtensions.PrependIf` on google.com</a></footer>
        public static IEnumerable<T> PrependIf<T>(
            this IEnumerable<T> source,
            Func<IEnumerable<T>, bool> condition,
            T prepend)
        {
            if (condition(source))
            {
                yield return prepend;
            }

            foreach (var obj in source)
            {
                yield return obj;
            }
        }

        /// <summary>
        /// Add a collection to the beginning of another collection, if a condition is met.
        /// </summary>
        /// <param name="source">The collection.</param>
        /// <param name="condition">The condition.</param>
        /// <param name="prepend">The collection to prepend.</param>
        /// <footer><a href="https://www.google.com/search?q=Sirenix.Utilities.LinqExtensions.PrependIf">`LinqExtensions.PrependIf` on google.com</a></footer>
        public static IEnumerable<T> PrependIf<T>(
            this IEnumerable<T> source,
            Func<IEnumerable<T>, bool> condition,
            IEnumerable<T> prepend)
        {
            if (condition(source))
            {
                foreach (var obj in prepend)
                {
                    yield return obj;
                }
            }

            foreach (var obj in source)
            {
                yield return obj;
            }
        }

        /// <summary>Add an item to the end of a collection.</summary>
        /// <param name="source">The collection.</param>
        /// <param name="append">Func to create the item to append.</param>
        /// <footer><a href="https://www.google.com/search?q=Sirenix.Utilities.LinqExtensions.AppendWith">`LinqExtensions.AppendWith` on google.com</a></footer>
        public static IEnumerable<T> AppendWith<T>(
            this IEnumerable<T> source,
            Func<T> append)
        {
            foreach (var obj in source)
            {
                yield return obj;
            }

            yield return append();
        }

        /// <summary>Add an item to the end of a collection.</summary>
        /// <param name="source">The collection.</param>
        /// <param name="append">The item to append.</param>
        /// <footer><a href="https://www.google.com/search?q=Sirenix.Utilities.LinqExtensions.AppendWith">`LinqExtensions.AppendWith` on google.com</a></footer>
        public static IEnumerable<T> AppendWith<T>(this IEnumerable<T> source, T append)
        {
            foreach (var obj in source)
            {
                yield return obj;
            }

            yield return append;
        }

        /// <summary>Add a collection to the end of another collection.</summary>
        /// <param name="source">The collection.</param>
        /// <param name="append">The collection to append.</param>
        /// <footer><a href="https://www.google.com/search?q=Sirenix.Utilities.LinqExtensions.AppendWith">`LinqExtensions.AppendWith` on google.com</a></footer>
        public static IEnumerable<T> AppendWith<T>(
            this IEnumerable<T> source,
            IEnumerable<T> append)
        {
            foreach (var obj in source)
            {
                yield return obj;
            }

            foreach (var obj in append)
            {
                yield return obj;
            }
        }

        /// <summary>
        /// Add an item to the end of a collection if a condition is met.
        /// </summary>
        /// <param name="source">The collection.</param>
        /// <param name="condition">The condition.</param>
        /// <param name="append">Func to create the item to append.</param>
        /// <footer><a href="https://www.google.com/search?q=Sirenix.Utilities.LinqExtensions.AppendIf">`LinqExtensions.AppendIf` on google.com</a></footer>
        public static IEnumerable<T> AppendIf<T>(
            this IEnumerable<T> source,
            bool condition,
            Func<T> append)
        {
            foreach (var obj in source)
            {
                yield return obj;
            }

            if (condition)
            {
                yield return append();
            }
        }

        /// <summary>
        /// Add an item to the end of a collection if a condition is met.
        /// </summary>
        /// <param name="source">The collection.</param>
        /// <param name="condition">The condition.</param>
        /// <param name="append">The item to append.</param>
        /// <footer><a href="https://www.google.com/search?q=Sirenix.Utilities.LinqExtensions.AppendIf">`LinqExtensions.AppendIf` on google.com</a></footer>
        public static IEnumerable<T> AppendIf<T>(
            this IEnumerable<T> source,
            bool condition,
            T append)
        {
            foreach (var obj in source)
            {
                yield return obj;
            }

            if (condition)
            {
                yield return append;
            }
        }

        /// <summary>
        /// Add a collection to the end of another collection if a condition is met.
        /// </summary>
        /// <param name="source">The collection.</param>
        /// <param name="condition">The condition.</param>
        /// <param name="append">The collection to append.</param>
        /// <footer><a href="https://www.google.com/search?q=Sirenix.Utilities.LinqExtensions.AppendIf">`LinqExtensions.AppendIf` on google.com</a></footer>
        public static IEnumerable<T> AppendIf<T>(
            this IEnumerable<T> source,
            bool condition,
            IEnumerable<T> append)
        {
            foreach (var obj in source)
            {
                yield return obj;
            }

            if (condition)
            {
                foreach (var obj in append)
                {
                    yield return obj;
                }
            }
        }

        /// <summary>
        /// Add an item to the end of a collection if a condition is met.
        /// </summary>
        /// <param name="source">The collection.</param>
        /// <param name="condition">The condition.</param>
        /// <param name="append">Func to create the item to append.</param>
        /// <footer><a href="https://www.google.com/search?q=Sirenix.Utilities.LinqExtensions.AppendIf">`LinqExtensions.AppendIf` on google.com</a></footer>
        public static IEnumerable<T> AppendIf<T>(
            this IEnumerable<T> source,
            Func<bool> condition,
            Func<T> append)
        {
            foreach (var obj in source)
            {
                yield return obj;
            }

            if (condition())
            {
                yield return append();
            }
        }

        /// <summary>
        /// Add an item to the end of a collection if a condition is met.
        /// </summary>
        /// <param name="source">The collection.</param>
        /// <param name="condition">The condition.</param>
        /// <param name="append">The item to append.</param>
        /// <footer><a href="https://www.google.com/search?q=Sirenix.Utilities.LinqExtensions.AppendIf">`LinqExtensions.AppendIf` on google.com</a></footer>
        public static IEnumerable<T> AppendIf<T>(
            this IEnumerable<T> source,
            Func<bool> condition,
            T append)
        {
            foreach (var obj in source)
            {
                yield return obj;
            }

            if (condition())
            {
                yield return append;
            }
        }

        /// <summary>
        /// Add a collection to the end of another collection if a condition is met.
        /// </summary>
        /// <param name="source">The collection.</param>
        /// <param name="condition">The condition.</param>
        /// <param name="append">The collection to append.</param>
        /// <footer><a href="https://www.google.com/search?q=Sirenix.Utilities.LinqExtensions.AppendIf">`LinqExtensions.AppendIf` on google.com</a></footer>
        public static IEnumerable<T> AppendIf<T>(
            this IEnumerable<T> source,
            Func<bool> condition,
            IEnumerable<T> append)
        {
            foreach (var obj in source)
            {
                yield return obj;
            }

            if (condition())
            {
                foreach (var obj in append)
                {
                    yield return obj;
                }
            }
        }

        /// <summary>
        /// Returns and casts only the items of type <typeparamref name="T" />.
        /// </summary>
        /// <param name="source">The collection.</param>
        /// <footer><a href="https://www.google.com/search?q=Sirenix.Utilities.LinqExtensions.FilterCast">`LinqExtensions.FilterCast` on google.com</a></footer>
        public static IEnumerable<T> FilterCast<T>(this IEnumerable source)
        {
            foreach (var obj1 in source)
            {
                if (obj1 is T obj)
                {
                    yield return obj;
                }
            }
        }

        /// <summary>Adds a collection to a hashset.</summary>
        /// <param name="hashSet">The hashset.</param>
        /// <param name="range">The collection.</param>
        /// <footer><a href="https://www.google.com/search?q=Sirenix.Utilities.LinqExtensions.AddRange">`LinqExtensions.AddRange` on google.com</a></footer>
        public static void AddRange<T>(this HashSet<T> hashSet, IEnumerable<T> range)
        {
            foreach (var obj in range)
            {
                hashSet.Add(obj);
            }
        }

        /// <summary>
        /// Returns <c>true</c> if the list is either null or empty. Otherwise <c>false</c>.
        /// </summary>
        /// <param name="list">The list.</param>
        /// <footer><a href="https://www.google.com/search?q=Sirenix.Utilities.LinqExtensions.IsNullOrEmpty">`LinqExtensions.IsNullOrEmpty` on google.com</a></footer>
        public static bool IsNullOrEmpty<T>(this IList<T> list)
        {
            return list == null || list.Count == 0;
        }

        /// <summary>Sets all items in the list to the given value.</summary>
        /// <param name="list">The list.</param>
        /// <param name="item">The value.</param>
        /// <footer><a href="https://www.google.com/search?q=Sirenix.Utilities.LinqExtensions.Populate">`LinqExtensions.Populate` on google.com</a></footer>
        public static void Populate<T>(this IList<T> list, T item)
        {
            var count = list.Count;

            for (var index = 0; index < count; ++index)
            {
                list[index] = item;
            }
        }

        /// <summary>
        /// Adds the elements of the specified collection to the end of the IList&lt;T&gt;.
        /// </summary>
        /// <footer><a href="https://www.google.com/search?q=Sirenix.Utilities.LinqExtensions.AddRange">`LinqExtensions.AddRange` on google.com</a></footer>
        public static void AddRange<T>(this IList<T> list, IEnumerable<T> collection)
        {
            if (list is List<T>)
            {
                ((List<T>) list).AddRange(collection);
            }
            else
            {
                foreach (var obj in collection)
                {
                    list.Add(obj);
                }
            }
        }

        /// <summary>Sorts an IList</summary>
        /// <footer><a href="https://www.google.com/search?q=Sirenix.Utilities.LinqExtensions.Sort">`LinqExtensions.Sort` on google.com</a></footer>
        public static void Sort<T>(this IList<T> list, Comparison<T> comparison)
        {
            if (list is List<T>)
            {
                ((List<T>) list).Sort(comparison);
            }
            else
            {
                var objList = new List<T>((IEnumerable<T>) list);
                objList.Sort(comparison);

                for (var index = 0; index < list.Count; ++index)
                {
                    list[index] = objList[index];
                }
            }
        }

        /// <summary>Sorts an IList</summary>
        /// <footer><a href="https://www.google.com/search?q=Sirenix.Utilities.LinqExtensions.Sort">`LinqExtensions.Sort` on google.com</a></footer>
        public static void Sort<T>(this IList<T> list)
        {
            if (list is List<T>)
            {
                ((List<T>) list).Sort();
            }
            else
            {
                var objList = new List<T>((IEnumerable<T>) list);
                objList.Sort();

                for (var index = 0; index < list.Count; ++index)
                {
                    list[index] = objList[index];
                }
            }
        }

        /// <summary>Counts the number of items which matches the condition in the IList</summary>
        /// <footer><a href="https://www.google.com/search?q=Sirenix.Utilities.LinqExtensions.Sort">`LinqExtensions.Sort` on google.com</a></footer>
        public static int CountExtension<T>(this IList<T> list, Func<T, bool> condition)
        {
            var counter = 0;

            foreach (var item in list)
            {
                if (condition(item))
                {
                    counter++;
                }
            }

            return counter;
        }

        public static void AddRange(this IList list, IEnumerable items)
        {
            foreach (var item in items)
            {
                list.Add(item);
            }
        }

        public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                collection.Add(item);
            }
        }

        public static void AddMultipleRanges<T>(this ICollection<T> collection, params ICollection<T>[] collectionsToMerge)
        {
            for (var i = 0; i < collectionsToMerge.Length; i++)
            {
                collection.AddRange(collectionsToMerge[i]);
            }
        }

        public static void Combine<T>(this ICollection<T> collection, ICollection<T> items)
        {
            foreach (var item in items)
            {
                if (!collection.Contains(item))
                {
                    collection.Add(item);
                }
            }
        }
    }
}