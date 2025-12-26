using System;
using System.Collections.Generic;

namespace Core.Scripts.Extensions.Linq
{
    public partial class LinqExtensions
    {
        public static void RemoveAll<T>(this LinkedList<T> list, Predicate<T> condition)
        {
            var node = list.First;
            while (node != null)
            {
                var next = node.Next;
                if (condition(node.Value))
                {
                    list.Remove(node);
                }
                node = next;
            }
        }
    }
}