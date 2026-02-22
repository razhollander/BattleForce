using System;
using System.Collections.Generic;
using CoreDomain.Scripts.Services.Logger.Base;

namespace Core.Scripts.Extensions.Linq
{
    public static partial class LinqExtensions
    {
        public static T FindWithId<T>(this T[] source, ushort id) where T : IEquatable<ushort>
        {
            for (int i = 0; i < source.Length; i++)
            {
                var environmentSpring = source[i];

                if (environmentSpring.Equals(id))
                {
                    return environmentSpring;
                }
            }

            LogService.LogError("No found with id: " + id + "");
            return default;
        }
        
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
    }
}