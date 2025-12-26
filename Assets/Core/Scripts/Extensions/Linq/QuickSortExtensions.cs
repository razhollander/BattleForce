using System;

namespace Core.Scripts.Extensions.Linq
{
    public static partial class LinqExtensions
    {
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
    }
}