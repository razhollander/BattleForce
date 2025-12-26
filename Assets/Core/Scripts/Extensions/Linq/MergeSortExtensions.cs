using System;

namespace Core.Scripts.Extensions.Linq
{
    public static partial class LinqExtensions
    {
        private static void MergeSort<T, TKey>(T[] arr, Func<T, TKey> keySelector) where TKey : IComparable<TKey>
        {
            if (arr.Length <= 1)
                return;

            var middle = arr.Length / 2;
            var low = new T[middle];
            var high = new T[arr.Length - middle];

            for (var i = 0; i < middle; i++)
                low[i] = arr[i];
            for (var i = middle; i < arr.Length; i++)
                high[i - middle] = arr[i];

            MergeSort(low, keySelector);
            MergeSort(high, keySelector);

            Merge(arr, keySelector, low, high);
        }

        private static void Merge<T, TKey>(T[] arr, Func<T, TKey> keySelector, T[] low, T[] high) where TKey : IComparable<TKey>
        {
            var leftIndex = 0;
            var rightIndex = 0;
            var mergedIndex = 0;

            while (leftIndex < low.Length && rightIndex < high.Length)
            {
                var pivot = keySelector(high[rightIndex]);
                if (keySelector(low[leftIndex]).CompareTo(pivot) <= 0)
                {
                    arr[mergedIndex] = low[leftIndex];
                    leftIndex++;
                }
                else
                {
                    arr[mergedIndex] = high[rightIndex];
                    rightIndex++;
                }
                mergedIndex++;
            }

            while (leftIndex < low.Length)
            {
                arr[mergedIndex] = low[leftIndex];
                leftIndex++;
                mergedIndex++;
            }

            while (rightIndex < high.Length)
            {
                arr[mergedIndex] = high[rightIndex];
                rightIndex++;
                mergedIndex++;
            }
        }
    }
}