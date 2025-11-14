using System.Collections.Generic;

namespace CoreDomain.Scripts.Helpers.Pools
{
    public abstract class BasePool<TPoolable> : IPool<TPoolable> where TPoolable : IPoolable
    {
        private readonly int _increaseStepAmount;
        private Queue<TPoolable> _pool;
        private readonly int _initialAmount;

        public BasePool(PoolData poolData)
        {
            _increaseStepAmount = poolData.IncreaseStepAmount;
            _initialAmount = poolData.InitialAmount;
        }

        public virtual void InitPool()
        {
            _pool = new();
            AddInstancesToQueue(_initialAmount);
        }

        private void AddInstancesToQueue(int instancesAmount)
        {
            var poolableInstances = CreatePoolableInstances(instancesAmount);
            poolableInstances.ForEach(poolable => _pool.Enqueue(poolable));
        }
        
        protected abstract List<TPoolable> CreatePoolableInstances(int instancesAmount);
        
        public TPoolable Spawn()
        {
            TPoolable obj;

            if (_pool.Count <= 0)
            {
                AddInstancesToQueue(_increaseStepAmount);
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