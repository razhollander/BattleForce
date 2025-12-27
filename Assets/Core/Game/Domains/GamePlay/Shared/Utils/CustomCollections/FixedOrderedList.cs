using System;
using System.Runtime.CompilerServices;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.ContextInstaller.Utils
{
    /// <summary>
    /// A fixed-capacity, non-alloc list backed by a constant-size array.
    /// No resizing, no allocations after construction.
    /// Has worse performacne than FixedUnorderedList when removing an item, but keeps the order of the items
    /// </summary>
    public sealed class FixedOrderedList<T>
    {
        private readonly T[] _items;
        private int _count;

        public FixedOrderedList(int capacity)
        {
            if (capacity <= 0) throw new ArgumentOutOfRangeException(nameof(capacity));
            _items = new T[capacity];
            _count = 0;
        }

        /// <summary>Maximum number of elements this list can hold.</summary>
        public int Capacity => _items.Length;

        /// <summary>Current number of elements in the list.</summary>
        public int Count => _count;

        /// <summary>Returns true if the list is full.</summary>
        public bool IsFull => _count >= _items.Length;

        /// <summary>Returns true if the list is empty.</summary>
        public bool IsEmpty => _count == 0;

        /// <summary>Direct indexed access (no bounds checks in release if used carefully).</summary>
        public T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if ((uint) index >= (uint) _count) throw new ArgumentOutOfRangeException(nameof(index));

                return _items[index];
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                if ((uint) index >= (uint) _count) throw new ArgumentOutOfRangeException(nameof(index));
                _items[index] = value;
            }
        }

        /// <summary>
        /// Adds an item. Throws if full.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(T item)
        {
            if (_count >= _items.Length)
                throw new InvalidOperationException($"FixedList<{typeof(T).Name}> is full (Capacity={Capacity}).");

            _items[_count++] = item;
        }

        // /// <summary>
        // /// Adds an item if there is space, returns false if full (no exception).
        // /// </summary>
        // [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // public bool TryAdd(T item)
        // {
        //     if (_count >= _items.Length) return false;
        //     _items[_count++] = item;
        //     return true;
        // }

        /// <summary>
        /// Clears the list. Optionally clears references to allow GC of referenced objects.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear(bool clearReferences = false)
        {
            if (clearReferences && RuntimeHelpers.IsReferenceOrContainsReferences<T>())
                Array.Clear(_items, 0, _count);

            _count = 0;
        }

        /// <summary>
        /// Removes element at index by shifting everything left (preserves order).
        /// O(n).
        /// </summary>
        public void RemoveAt(int index, bool clearReferences = false)
        {
            if ((uint) index >= (uint) _count) throw new ArgumentOutOfRangeException(nameof(index));

            int moveCount = _count - index - 1;

            if (moveCount > 0)
                Array.Copy(_items, index + 1, _items, index, moveCount);

            _count--;

            if (clearReferences && RuntimeHelpers.IsReferenceOrContainsReferences<T>())
                _items[_count] = default;
        }

        /// <summary>
        /// Returns a Span over current items (no allocation).
        /// Great for fast loops.
        /// </summary>
        public Span<T> AsSpan() => new Span<T>(_items, 0, _count);

        /// <summary>
        /// Returns the raw internal array (do NOT modify beyond Count).
        /// </summary>

        //public T[] RawArray => _items;

        // ---------- Non-alloc enumeration ----------
        //public Enumerator GetEnumerator() => new Enumerator(_items, _count);

        public struct Enumerator
        {
            private readonly T[] _arr;
            private readonly int _count;
            private int _index;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Enumerator(T[] arr, int count)
            {
                _arr = arr;
                _count = count;
                _index = -1;
            }

            public T Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _arr[_index];
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext()
            {
                int next = _index + 1;

                if (next >= _count) return false;
                _index = next;

                return true;
            }
        }
    }
}
