using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using CoreDomain.Scripts.Services.Logger.Base;

namespace Core.Scripts.Utils
{
    public class ConcurrentPool<T>
    {
        // Mirror is single threaded, no need for concurrent collections
        // concurrent bag is for items who's order doesn't matter.
        // just about right for our use case here.
        readonly ConcurrentBag<T> objects = new ConcurrentBag<T>();

        // some types might need additional parameters in their constructor, so
        // we use a Func<T> generator
        readonly Func<T> objectGenerator;

        public ConcurrentPool(Func<T> objectGenerator, int initialCapacity)
        {
            this.objectGenerator = objectGenerator;

            // allocate an initial pool so we have fewer (if any)
            // allocations in the first few frames (or seconds).
            for (int i = 0; i < initialCapacity; ++i)
                objects.Add(objectGenerator());
        }

        // take an element from the pool, or create a new one if empty
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Get()
        {
            if (objects.TryTake(out T obj))
            {
                return obj;
            }
            
            LogService.LogError($"ConcurrentPool: No objects available of type {typeof(T)}, increasing capacity by 1!");
            return objectGenerator();
        }

        // return an element to the pool
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Return(T item) => objects.Add(item);

        // count to see how many objects are in the pool. useful for tests.
        public int Count => objects.Count;
    }
}