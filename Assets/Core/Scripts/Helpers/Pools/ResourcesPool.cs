using System.Collections.Generic;
using CoreDomain.Scripts.Services.ResourcesLoaderService;
using UnityEngine;
using Zenject;

namespace CoreDomain.Scripts.Helpers.Pools
{
    public abstract class ResourcesPool<TPoolable> : BasePool<TPoolable>, IResourcesPool<TPoolable> where TPoolable : MonoBehaviour, IPoolable
    {
        private readonly DiContainer _diContainer;
        private readonly IResourcesLoaderService _resourcesLoaderService;
        public abstract string AssetPath { get; }
        protected abstract string ParentGameObjectName { get; }
        private Transform _parentTransform;

        public ResourcesPool(PoolData poolData, DiContainer diContainer, IResourcesLoaderService resourcesLoaderService) : base(poolData)
        {
            _diContainer = diContainer;
            _resourcesLoaderService = resourcesLoaderService;
        }

        public override void InitPool()
        {
            _parentTransform = new GameObject(ParentGameObjectName).transform;
            base.InitPool();
        }

        protected override List<TPoolable> CreatePoolableInstances(int instancesAmount)
        {
            var poolables = new List<TPoolable>();
            var asset = _resourcesLoaderService.Load<TPoolable>(AssetPath);

            for (int i = 0; i < instancesAmount; i++)
            {
                var poolable = _diContainer.InstantiatePrefab(asset.gameObject);
                poolable.SetActive(false);
                poolable.transform.SetParent(_parentTransform);
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

            obj.transform.SetParent(_parentTransform);
            base.Despawn(obj);
        }
    }
}