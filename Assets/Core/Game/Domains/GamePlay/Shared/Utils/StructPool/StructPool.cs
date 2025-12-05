using System;
using System.Collections.Generic;
using CoreDomain.Scripts.Services.Logger.Base;

public sealed class StructPool<T> where T : struct
{
    private readonly T[] _items;
    private readonly Stack<int> _freeSlots;
    private readonly bool[] _inUse;

    public int Capacity => _items.Length;
    public int UsedCount;
    public StructPool(int capacity)
    {
        if (capacity <= 0)
            throw new ArgumentOutOfRangeException(nameof(capacity));

        _items = new T[capacity];
        _inUse = new bool[capacity];
        _freeSlots = new Stack<int>(capacity);

        for (int i = 0; i < capacity; i++)
            _freeSlots.Push(i);
    }

    /// <summary>
    /// Try to reserve a slot in the pool.
    /// You get an index which you can use to access the item by ref.
    /// </summary>
    public void Rent(out int index)
    {
        if (_freeSlots.Count == 0)
        {
            index = -1;
            LogService.LogError("Not enough available bullets!");
        }
        
        index = _freeSlots.Pop();
        _inUse[index] = true;
        UsedCount++;
        // Optional: clear to default for safety
        _items[index] = default;
    }

    /// <summary>
    /// Return the slot to the pool. Caller promises not to use that index again
    /// unless it is re-rented.
    /// </summary>
    public void Return(int index)
    {
        if ((uint)index >= (uint)_items.Length)
            throw new ArgumentOutOfRangeException(nameof(index));

        if (!_inUse[index])
            return; // already free, ignore or throw based on how strict you want to be

        _inUse[index] = false;
        UsedCount--;
        _freeSlots.Push(index);
    }

    /// <summary>
    /// Direct ref access to the item stored at the given index.
    /// </summary>
    public ref T this[int index]
    {
        get
        {
            if ((uint)index >= (uint)_items.Length)
                throw new ArgumentOutOfRangeException(nameof(index));

            return ref _items[index];
        }
    }

    /// <summary>
    /// Returns true if the slot is currently rented.
    /// </summary>
    public bool IsInUse(int index)
    {
        if ((uint)index >= (uint)_items.Length)
            throw new ArgumentOutOfRangeException(nameof(index));

        return _inUse[index];
    }

    /// <summary>
    /// Enumerate all indices that are currently in use.
    /// </summary>
    public IEnumerable<int> UsedIndices()
    {
        for (int i = 0; i < _items.Length; i++)
        {
            if (_inUse[i])
                yield return i;
        }
    }
}
