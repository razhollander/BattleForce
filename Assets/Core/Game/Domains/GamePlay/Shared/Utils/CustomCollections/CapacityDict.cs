using System;
using System.Collections;
using System.Collections.Generic;
using CoreDomain.Scripts.Services.Logger.Base;

public sealed class CapacityDict<TKey, TValue> : IDictionary<TKey, TValue>
{
    private readonly Dictionary<TKey, TValue> _dict;
    private readonly int _initialCapacity;

    // Avoid log spam
    private bool _loggedInitialCapacityExceeded;

    public CapacityDict(int capacity)
    {
        if (capacity < 0) throw new ArgumentOutOfRangeException(nameof(capacity));
        _initialCapacity = capacity;
        _dict = new Dictionary<TKey, TValue>(capacity);
    }

    public CapacityDict(int capacity, IEqualityComparer<TKey> comparer)
    {
        if (capacity < 0) throw new ArgumentOutOfRangeException(nameof(capacity));
        _initialCapacity = capacity;
        _dict = new Dictionary<TKey, TValue>(capacity, comparer);
    }

    // -------------------- IDictionary API --------------------

    public int Count => _dict.Count;
    public bool IsReadOnly => ((IDictionary<TKey, TValue>)_dict).IsReadOnly;

    public ICollection<TKey> Keys => _dict.Keys;
    public ICollection<TValue> Values => _dict.Values;

    public TValue this[TKey key]
    {
        get => _dict[key];
        set
        {
            // Setting indexer may add a new key, so only check when key doesn't exist
            bool addingNew = !_dict.ContainsKey(key);
            if (addingNew)
                LogIfInitialCapacityExceeded(_dict.Count + 1);

            _dict[key] = value;
        }
    }

    public void Add(TKey key, TValue value)
    {
        LogIfInitialCapacityExceeded(_dict.Count + 1);
        _dict.Add(key, value);
    }

    public bool TryAdd(TKey key, TValue value)
    {
#if NETSTANDARD2_1_OR_GREATER || UNITY_2021_2_OR_NEWER
        if (!_dict.ContainsKey(key))
            LogIfInitialCapacityExceeded(_dict.Count + 1);

        return _dict.TryAdd(key, value);
#else
        if (_dict.ContainsKey(key))
            return false;

        LogIfInitialCapacityExceeded(_dict.Count + 1);
        _dict.Add(key, value);
        return true;
#endif
    }

    public bool ContainsKey(TKey key) => _dict.ContainsKey(key);

    public bool Remove(TKey key) => _dict.Remove(key);

    public bool TryGetValue(TKey key, out TValue value) => _dict.TryGetValue(key, out value);

    public void Add(KeyValuePair<TKey, TValue> item)
    {
        LogIfInitialCapacityExceeded(_dict.Count + 1);
        ((IDictionary<TKey, TValue>)_dict).Add(item);
    }

    public void Clear()
    {
        _dict.Clear();
        _loggedInitialCapacityExceeded = false; // reset warning for next usage cycle
    }

    public bool Contains(KeyValuePair<TKey, TValue> item) => ((IDictionary<TKey, TValue>)_dict).Contains(item);

    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        => ((IDictionary<TKey, TValue>)_dict).CopyTo(array, arrayIndex);

    public bool Remove(KeyValuePair<TKey, TValue> item) => ((IDictionary<TKey, TValue>)_dict).Remove(item);

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => _dict.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_dict).GetEnumerator();

    // -------------------- Logging --------------------

    private void LogIfInitialCapacityExceeded(int requestedCount)
    {
        if (_initialCapacity == 0) return; // user didn't want preallocation

        if (!_loggedInitialCapacityExceeded && requestedCount > _initialCapacity)
        {
            _loggedInitialCapacityExceeded = true;
            LogInitialCapacityExceeded(_initialCapacity, requestedCount);
        }
    }

    private static void LogInitialCapacityExceeded(int initialCapacity, int requestedCount)
    {
        LogService.LogError(
            $"[CapacityLoggingDictionary] Initial capacity exceeded! " +
            $"InitialCapacity={initialCapacity}, RequestedCount={requestedCount}. " +
            $"Dictionary will resize + rehash (alloc). Consider preallocating higher.");
    }
}
