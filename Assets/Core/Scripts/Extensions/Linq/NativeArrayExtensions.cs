using System;
using System.Collections.Generic;
using Unity.Collections;

namespace Core.Scripts.Extensions.Linq
{
    public static partial class LinqExtensions
    {
        public static bool Any<T>(this NativeArray<T> source) where T : struct
        {
            return source.Length > 0;
        }
        
        public static bool Any<T>(this NativeArray<T> source, Func<T, bool> predicate) where T : struct
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
        
        public static List<T> ToList<T>(this NativeArray<T> nativeArray) where T : struct
        {
            var list = new List<T>(nativeArray.Length);
    
            for (var i = 0; i < nativeArray.Length; i++)
            {
                list.Add(nativeArray[i]);
            }

            return list;
        }
        
        public static T FirstOrDefault<T>(this NativeArray<T> source, Func<T, bool> predicate) where T : struct
        {
            for (var i = 0; i < source.Length; i++)
            {
                var item = source[i];
                if (predicate(item))
                {
                    return item;
                }
            }

            return default;
        }
        
        public static NativeArray<T> Where<T>(this NativeArray<T> source, Func<T, bool> predicate, Allocator allocator) where T : struct
        {
            var count = 0;

            for (var i = 0; i < source.Length; i++)
            {
                if (predicate(source[i]))
                {
                    count++;
                }
            }

            var result = new NativeArray<T>(count, allocator);

            var resultIndex = 0;

            for (var i = 0; i < source.Length; i++)
            {
                if (predicate(source[i]))
                {
                    result[resultIndex] = source[i];
                    resultIndex++;
                }
            }

            return result;
        }
        
        public static NativeArray<TResult> Select<TSource, TResult>(this NativeArray<TSource> source, Func<TSource, TResult> selector, Allocator allocator) where TSource : struct where TResult : struct
        {
            var result = new NativeArray<TResult>(source.Length, allocator);

            for (var i = 0; i < source.Length; i++)
            {
                result[i] = selector(source[i]);
            }

            return result;
        }
        
        public static int Count<T>(this NativeArray<T> source, Func<T, bool> predicate) where T : struct
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
        
        public static bool Contains<T>(this NativeArray<T> source, T target) where T : struct
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
    }
}