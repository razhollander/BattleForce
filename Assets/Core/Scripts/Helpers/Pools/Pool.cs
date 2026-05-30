using System.Collections.Generic;
using CoreDomain.Scripts.Services.Logger.Base;

namespace CoreDomain.Scripts.Helpers.Pools
{
    public abstract class BasePool<TPoolable> : IPool<TPoolable> where TPoolable : IPoolable
    {
        private readonly int _increaseStepAmount;
        private Queue<TPoolable> _pool;
        private readonly int _initialAmount;
        private bool _initialized;

        public BasePool(PoolData poolData)
        {
            _increaseStepAmount = poolData.IncreaseStepAmount;
            _initialAmount = poolData.InitialAmount;
        }

        public virtual void InitPool()
        {
            _initialized = true;
            _pool = new(_initialAmount);
            AddInstancesToQueue(_initialAmount);
        }

        public virtual void DisposePool()
        {
            _initialized = false;
            DestroyPoolableInstances(_pool);
        }
        
        private void AddInstancesToQueue(int instancesAmount)
        {
            var poolableInstances = CreatePoolableInstances(instancesAmount);
            poolableInstances.ForEach(poolable => _pool.Enqueue(poolable));
        }
        
        protected abstract List<TPoolable> CreatePoolableInstances(int instancesAmount);
        protected abstract void DestroyPoolableInstances(Queue<TPoolable> instances);
        
        public TPoolable Spawn()
        {
            if (!_initialized)
            {
                LogService.LogError("Pool is not initialized!");
                return default;
            }
            
            TPoolable obj;

            if (_pool.Count <= 0)
            {
                AddInstancesToQueue(_increaseStepAmount);
            }
            
            obj = _pool.Dequeue();
            obj.Despawn = InternalDespawn;
            obj.OnSpawned();
            return obj;

            void InternalDespawn()
            {
                Despawn(obj);
            }
        }

        protected virtual void Despawn(TPoolable obj)
        {
            obj.OnDespawned();
            _pool.Enqueue(obj);
        }
    }
}