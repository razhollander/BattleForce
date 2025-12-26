using System;
using System.Collections.Generic;
using Core.Scripts.Extensions.Linq.Enums;

namespace Core.Scripts.Extensions.Linq
{
    public static partial class LinqExtensions
    {
        public static bool Any<T>(this List<T> source)
        {
            return source.Count > 0;
        }

        public static void MoveItemToStart<T>(this List<T> source, Func<T, bool> condition)
        {
            var item = source.FirstOrDefault(condition);
            if (item == null)
            {
                return;
            }

            source.Remove(item);
            source.Insert(0, item);
        }
        
        public static bool All<T>(this List<T> source, Func<T, bool> predicate)
        {
            for (var i = 0; i < source.Count; i++)
            {
                if (!predicate(source[i]))
                {
                    return false;
                }
            }
            return true;
        }

        public static bool Any<T>(this List<T> source, Func<T, bool> predicate)
        {
            for (var i = 0; i < source.Count; i++)
            {
                if (predicate(source[i]))
                {
                    return true;
                }
            }
            return false;
        }

        public static int Count<T>(this IEnumerable<T> source, Func<T, bool> predicate)
        {
            var count = 0;
            foreach (var item in source)
            {
                if (predicate(item))
                {
                    count++;
                }
            }
            return count;
        }

        public static T FirstOrDefault<T>(this List<T> source, Func<T, bool> predicate)
        {
            for (var i = 0; i < source.Count; i++)
            {
                if (predicate(source[i]))
                {
                    return source[i];
                }
            }
            return default;
        }

        public static T FirstOrDefault<T>(this List<T> list)
        {
            if (list == null || list.Count == 0)
            {
                return default;
            }

            return list[0];
        }

        public static T Max<T>(this List<T> source) where T : IComparable<T>
        {
            var max = default(T);
            for (var i = 0; i < source.Count; i++)
            {
                if(source[i].CompareTo(max) > 0)
                {
                    max = source[i];
                }
            }

            return max;
        }

        public static List<T> OrderBy<T, TKey>(this List<T> source, Func<T, TKey> keySelector, SortType sortType = SortType.MergeSort) where TKey : IComparable<TKey>
        {
            var arr = source.ToArray();
            switch (sortType)
            {
                case SortType.QuickSort:
                    QuickSort(arr, keySelector, 0, arr.Length - 1);
                    break;
                case SortType.MergeSort:
                default:
                    MergeSort(arr, keySelector);
                    break;
            }

            return new List<T>(arr);
        }

        public static void RemoveAll<T>(this List<T> list, Predicate<T> condition)
        {
            var writeIndex = 0;
            for (var readIndex = 0; readIndex < list.Count; readIndex++)
            {
                if (!condition(list[readIndex]))
                {
                    list[writeIndex] = list[readIndex];
                    writeIndex++;
                }
            }

            if (writeIndex < list.Count)
            {
                list.RemoveRange(writeIndex, list.Count - writeIndex);
            }
        }

        public static List<TResult> Select<TSource, TResult>(this List<TSource> source, Func<TSource, TResult> selector)
        {
            var result = new List<TResult>(source.Count);
        
            for (var i = 0; i < source.Count; i++)
            {
                result.Add(selector(source[i]));
            }
        
            return result;
        }

        public static List<T> Where<T>(this List<T> source, Func<T, bool> predicate)
        {
            var list = new List<T>(source.Count);

            for (var i = 0; i < source.Count; i++)
            {
                var item = source[i];
                if (predicate(item))
                {
                    list.Add(item);
                }
            }

            list.TrimExcess();
            return list;
        }
        
        public static List<T> Except<T>(this List<T> source, List<T> other)
        {
            var list = new List<T>(source.Count);

            for (var i = 0; i < source.Count; i++)
            {
                var item = source[i];
                if (!other.Contains(item))
                {
                    list.Add(item);
                }
            }

            list.TrimExcess();
            return list;
        }
        
        public static int Sum<T>(this List<T> source, Func<T, int> selector)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (selector == null)
            {
                throw new ArgumentNullException(nameof(selector));
            }

            var sum = 0;
            foreach (var item in source)
            {
                sum += selector(item);
            }

            return sum;
        }
        
        public static float Sum<T>(this List<T> source, Func<T, float> selector)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (selector == null)
            {
                throw new ArgumentNullException(nameof(selector));
            }

            var sum = 0.0f;
            foreach (var item in source)
            {
                sum += selector(item);
            }

            return sum;
        }
        
        public static List<TResult> SelectMany<TSource, TResult>(this List<TSource> source, Func<TSource, IEnumerable<TResult>> selector)
        {
            var result = new List<TResult>();
            foreach (var item in source)
            {
                result.AddRange(selector(item));
            }
            return result;
        }
    }
}