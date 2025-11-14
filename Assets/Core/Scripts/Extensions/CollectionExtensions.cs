using System.Collections.Generic;

namespace CoreDomain.Scripts.Extensions
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
    }
}