using System.Collections.Generic;
using UnityEngine;

namespace Core.Scripts.Extensions
{
    public static class CollectionExtensions
    {
        public static void RemoveElements<T>(this ICollection<T> list, ICollection<T> elementsToRemove)
        {
            foreach (var elementToRemove in elementsToRemove)
            {
                list.Remove(elementToRemove);
            }
        }

        public static bool IsNullOrEmpty<T>(this ICollection<T> list)
        {
            return list == null || list.Count == 0;
        }
        
        public static void Shuffle(this List<int> list)
        {
            for (var i = list.Count - 1; i > 0; i--)
            {
                var j = Random.Range(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }
}