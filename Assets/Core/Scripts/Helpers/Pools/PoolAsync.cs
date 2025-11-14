using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace CoreDomain.Scripts.Helpers.Pools
{
    public abstract class BasePoolAsync<TPoolable> : IPoolAsync<TPoolable> where TPoolable : IPoolable
    {
        private readonly int _increaseStepAmount;
        private Queue<TPoolable> _pool;
        private readonly int _initialAmount;

        public BasePoolAsync(PoolData poolData)
        {
            _increaseStepAmount = poolData.IncreaseStepAmount;
            _initialAmount = poolData.InitialAmount;
        }

        public virtual async Awaitable InitPool(CancellationTokenSource cancellationTokenSource)
        {
            _pool = new();
            await AddInstancesToQueue(_initialAmount, cancellationTokenSource);
        }

        private async Awaitable AddInstancesToQueue(int instancesAmount, CancellationTokenSource cancellationTokenSource)
        {
            var poolableInstances = await CreatePoolableInstances(instancesAmount, cancellationTokenSource);
            poolableInstances.ForEach(poolable => _pool.Enqueue(poolable));
        }
        
        protected abstract Awaitable<List<TPoolable>> CreatePoolableInstances(int instancesAmount, CancellationTokenSource cancellationTokenSource);
        
        public async Awaitable<TPoolable> Spawn(CancellationTokenSource cancellationTokenSource)
        {
            TPoolable obj;

            if (_pool.Count <= 0)
            {
                await AddInstancesToQueue(_increaseStepAmount, cancellationTokenSource);
            }
            
            obj = _pool.Dequeue();
            obj.Despawn = () => Despawn(obj);
            obj.OnSpawned();

            return obj;
        }

        protected virtual void Despawn(TPoolable obj)
        {
            obj.OnDespawned();
            _pool.Enqueue(obj);
        }
    }
}