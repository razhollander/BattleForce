using System;
using System.Collections.Generic;

namespace Core.Scripts.Extensions.Linq
{
    public static partial class LinqExtensions
    {
        public static bool Any<TKey, TValue>(this Dictionary<TKey, TValue> source)
        {
            return source.Count > 0;
        }

        public static bool Any<TKey, TValue>(this Dictionary<TKey, TValue> source, Func<KeyValuePair<TKey, TValue>, bool> predicate)
        {
            foreach (var item in source)
            {
                if (predicate(item))
                {
                    return true;
                }
            }
            return false;
        }

        public static int Count<TKey, TValue>(Dictionary<TKey, TValue> source, Func<KeyValuePair<TKey, TValue>, bool> predicate)
        {
            var count = 0;
            foreach (var kvp in source)
            {
                if (predicate(kvp))
                {
                    count++;
                }
            }
            return count;
        }
        
        public static KeyValuePair<TKey, TValue>? FirstOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> source, Func<KeyValuePair<TKey, TValue>, bool> predicate)
        {
            foreach (var kvp in source)
            {
                if (predicate(kvp))
                {
                    return kvp;
                }
            }
            return null;
        }

        public static Dictionary<TKeyResult, TValueResult> Select<TKeySource, TValueSource, TKeyResult, TValueResult>(
            this Dictionary<TKeySource, TValueSource> source, 
            Func<KeyValuePair<TKeySource, TValueSource>, KeyValuePair<TKeyResult, TValueResult>> selector)
        {
            var result = new Dictionary<TKeyResult, TValueResult>(source.Count);

            foreach (var kvp in source)
            {
                var transformedKvp = selector(kvp);
                result.Add(transformedKvp.Key, transformedKvp.Value);
            }

            return result;
        }

        public static Dictionary<TKey, TValue> Where<TKey, TValue>(this Dictionary<TKey, TValue> source, Func<KeyValuePair<TKey, TValue>, bool> predicate)
        {
            var dictionary = new Dictionary<TKey, TValue>(source.Count);
        
            foreach (var item in source)
            {
                if (predicate(item))
                {
                    dictionary[item.Key] = item.Value;
                }
            }
            
            dictionary.TrimExcess();
            return dictionary;
        }
        
        public static bool All<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, Func<KeyValuePair<TKey, TValue>, bool> predicate)
        {
            foreach (var kvp in dictionary)
            {
                if (!predicate(kvp))
                {
                    return false;
                }
            }
            return true;
        }
        
        public static int Sum<TKey, TValue>(this Dictionary<TKey, TValue>.ValueCollection source, Func<TValue, int> selector)
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
            
            foreach (var sourceValue in source)
            {
                sum += selector(sourceValue);
            }
            return sum;
        }
        
        public static Dictionary<TKey, TValue> ToDictionary<TSource, TKey, TValue>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            Func<TSource, TValue> valueSelector)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (keySelector == null) throw new ArgumentNullException(nameof(keySelector));
            if (valueSelector == null) throw new ArgumentNullException(nameof(valueSelector));

            var dictionary = new Dictionary<TKey, TValue>();
            foreach (var item in source)
            {
                dictionary.Add(keySelector(item), valueSelector(item));
            }
            return dictionary;
        }
    }
}