using System;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using Sirenix.Serialization;

namespace Core.Scripts.Utils.CustomCollections
{
    /// <summary>
    /// A fixed-capacity, non-alloc list backed by a constant-size array.
    /// No resizing, no allocations after construction.
    /// Has better performacne than FixedOrderedList when removing an item, but this also changes the order of items 
    /// </summary>
// [JsonConverter(typeof(FixedClassUnorderedListJsonConverter<>))]
    public sealed class FixedClassUnorderedList<T> where T : class
    {
        [JsonIgnore]
        private readonly T[] _items;
        private int _count;

        public FixedClassUnorderedList(int capacity, Func<T> factory)
        {
            if (capacity <= 0) throw new ArgumentOutOfRangeException(nameof(capacity));
            _items = new T[capacity];

            for (var i = 0; i < _items.Length; i++)
            {
                _items[i] = factory();
            }
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
                if ((uint)index >= (uint)_count) throw new ArgumentOutOfRangeException(nameof(index));
                return _items[index];
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                if ((uint)index >= (uint)_count) throw new ArgumentOutOfRangeException(nameof(index));
                _items[index] = value;
            }
        }

        /// <summary>
        /// Adds an item. Throws if full.
        /// </summary>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        // public void Add(T item)
        // {
        //     if (_count >= _items.Length)
        //         throw new InvalidOperationException($"FixedList<{typeof(T).Name}> is full (Capacity={Capacity}).");
        //
        //     _items[_count++] = item;
        // }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T AddAndGet()
        {
            if (_count >= _items.Length)
                throw new InvalidOperationException($"FixedList<{typeof(T).Name}> is full (Capacity={Capacity}).");

            int index = _count++;
            return _items[index];
        }
    
        public T GetByIndex(int index)
        {
            if (_count >= _items.Length)
                throw new InvalidOperationException($"FixedList<{typeof(T).Name}> is full (Capacity={Capacity}).");
        
            return _items[index];
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
        /// Removes element at index by swapping with last (does NOT preserve order).
        /// O(1).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveAt(int index, bool clearReferences = false)
        {
            if ((uint)index >= (uint)_count) throw new ArgumentOutOfRangeException(nameof(index));

            _count--;
            if (index != _count)
            {
                (_items[index], _items[_count]) = (_items[_count], _items[index]);
            }

            // If you need to clear references so GC can collect them, you would do it here:
            // if (clearReferences ...) _items[_count] = null; 
            // BUT NOTE: If you null it out, AddAndGet() will eventually return null 
            // instead of a pooled object, which breaks your factory pattern!
        }

        /// <summary>
        /// Returns a Span over current items (no allocation).
        /// Great for fast loops.
        /// </summary>
        public Span<T> AsSpan() => new Span<T>(_items, 0, _count);

        /// <summary>
        /// Returns the raw internal array (do NOT modify beyond Count).
        /// </summary>
        [JsonIgnore]
        public T[] RawArray => _items;

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
    
        // ------------------------------------------------------------
        // ✅ ODIN SERIALIZATION VIEW
        // ------------------------------------------------------------

        // This is what Odin will serialize (ONLY Count items)
        // Odin will ignore private fields unless they have OdinSerialize.
        [OdinSerialize]
        private T[] SerializedItems
        {
            get
            {
                // Return array with only used items
                var arr = new T[_count];
                Array.Copy(_items, 0, arr, 0, _count);
                return arr;
            }
            set
            {
                // Odin assigns this on deserialize
                _count = 0;

                if (value == null)
                    return;

                if (value.Length > _items.Length)
                    throw new InvalidOperationException(
                        $"Deserialized item count {value.Length} exceeds FixedClassUnorderedList capacity {_items.Length}");

                Array.Copy(value, 0, _items, 0, value.Length);
                _count = value.Length;
            }
        }
    }
}
