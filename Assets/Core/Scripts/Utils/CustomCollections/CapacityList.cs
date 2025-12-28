using System;
using System.Collections;
using System.Collections.Generic;
using CoreDomain.Scripts.Services.Logger.Base;

namespace Core.Scripts.Utils.CustomCollections
{
    public sealed class CapacityList<T> : IList<T>
    {
        private readonly List<T> _list;

        // Avoid log spam: log only once per capacity value
        private int _lastLoggedCapacity = -1;

        public CapacityList() => _list = new List<T>();
        public CapacityList(int capacity) => _list = new List<T>(capacity);

        // -------------------- List-like API --------------------

        public int Count => _list.Count;
        public int Capacity
        {
            get => _list.Capacity;
            set => _list.Capacity = value;
        }

        public bool IsReadOnly => ((ICollection<T>)_list).IsReadOnly;

        public T this[int index]
        {
            get => _list[index];
            set => _list[index] = value;
        }

        public void Add(T item)
        {
            // If Count == Capacity, the next Add will grow the internal array
            LogIfCapacityReached(Count, Capacity);
            _list.Add(item);
        }

        public void AddRange(IEnumerable<T> collection)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));

            // We can't know exact cost unless it's an ICollection<T>
            if (collection is ICollection<T> c)
            {
                // If Count + c.Count > Capacity, List will grow
                if (Count + c.Count > Capacity)
                    LogCapacityGrowth(Count, Capacity, Count + c.Count);
            }

            _list.AddRange(collection);
        }

        public void Insert(int index, T item)
        {
            LogIfCapacityReached(Count, Capacity);
            _list.Insert(index, item);
        }

        public void InsertRange(int index, IEnumerable<T> collection)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));

            if (collection is ICollection<T> c)
            {
                if (Count + c.Count > Capacity)
                    LogCapacityGrowth(Count, Capacity, Count + c.Count);
            }

            _list.InsertRange(index, collection);
        }

        public void Clear() => _list.Clear();

        public bool Remove(T item) => _list.Remove(item);

        public void RemoveAt(int index) => _list.RemoveAt(index);

        public int IndexOf(T item) => _list.IndexOf(item);

        public bool Contains(T item) => _list.Contains(item);

        public void CopyTo(T[] array, int arrayIndex) => _list.CopyTo(array, arrayIndex);

        public IEnumerator<T> GetEnumerator() => _list.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_list).GetEnumerator();

        public void Sort() => _list.Sort();
        public void Sort(IComparer<T> comparer) => _list.Sort(comparer);
        public void Sort(int index, int count, IComparer<T> comparer) => _list.Sort(index, count, comparer);

        public T[] ToArray() => _list.ToArray();

        public void TrimExcess() => _list.TrimExcess();

        // -------------------- Logging --------------------

        private void LogIfCapacityReached(int count, int capacity)
        {
            // When count == capacity, the *next* Add/Insert will force a growth allocation
            if (count >= capacity && capacity != _lastLoggedCapacity)
            {
                _lastLoggedCapacity = capacity;
                LogCapacityReached(count, capacity);
            }
        }

        private void LogCapacityGrowth(int count, int capacity, int requestedCount)
        {
            if (capacity != _lastLoggedCapacity)
            {
                _lastLoggedCapacity = capacity;
                LogCapacityWillGrow(count, capacity, requestedCount);
            }
        }

        private static void LogCapacityReached(int count, int capacity)
        {
            LogService.LogError($"[CapacityLoggingList] Capacity reached! Count={count}, Capacity={capacity}. Next Add/Insert will allocate.");
        }
    
        private static void LogCapacityWillGrow(int count, int capacity, int requestedCount)
        {
            LogService.LogError($"[CapacityLoggingList] Capacity will grow! Count={count}, Capacity={capacity}, RequestedCount={requestedCount}. This will allocate.");
        }
    }
}
