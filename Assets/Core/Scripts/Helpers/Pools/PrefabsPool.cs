using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace CoreDomain.Scripts.Helpers.Pools
{
    public abstract class PrefabsPool<TPoolable> : BasePool<TPoolable>, IPreafabsPool<TPoolable> where TPoolable : MonoBehaviour, IPoolable
    {
        protected abstract string ParentGameObjectName { get; }
        
        private readonly DiContainer _diContainer;
        private readonly TPoolable _prefab;
        private Transform _parentTransform;

        public PrefabsPool(PoolData poolData, DiContainer diContainer, TPoolable prefab) : base(poolData)
        {
            _diContainer = diContainer;
            _prefab = prefab;
        }

        public override void InitPool()
        {
            _parentTransform = new GameObject(ParentGameObjectName).transform;
            base.InitPool();
        }

        protected override List<TPoolable> CreatePoolableInstances(int instancesAmount)
        {
            var poolables = new List<TPoolable>();

            for (int i = 0; i < instancesAmount; i++)
            {
                var poolable = _diContainer.InstantiatePrefab(_prefab, _parentTransform);
                poolable.SetActive(false);
                var poolableComponent = poolable.GetComponent<TPoolable>();
                poolableComponent.OnCreated();
                poolables.Add(poolableComponent);
            }

            return poolables;
        }

        protected override void Despawn(TPoolable obj)
        {
            if (obj == null)
            {
                return;
            }

            obj.gameObject.SetActive(false);
            obj.transform.SetParent(_parentTransform);
            base.Despawn(obj);
        }
    }
}