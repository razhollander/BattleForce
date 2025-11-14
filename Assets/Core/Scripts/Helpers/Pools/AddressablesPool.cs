using System.Collections.Generic;
using System.Threading;
using CoreDomain.Scripts.Services.AddressablesLoader;
using CoreDomain.Scripts.Services.ResourcesLoaderService;
using UnityEngine;
using Zenject;

namespace CoreDomain.Scripts.Helpers.Pools
{
    public abstract class AddressablesPool<TPoolable> : BasePoolAsync<TPoolable>, IAddressablesPool<TPoolable> where TPoolable : MonoBehaviour, IPoolable
    {
        private readonly DiContainer _diContainer;
        private readonly IAddressablesLoaderService _addressablesLoaderService;
        private readonly IResourcesLoaderService _resourcesLoaderService;
        public abstract string AssetAdress { get; }

        protected abstract string ParentGameObjectName { get; }
        private Transform _parentTransform;

        public AddressablesPool(PoolData poolData, DiContainer diContainer, IAddressablesLoaderService addressablesLoaderService) : base(poolData)
        {
            _diContainer = diContainer;
            _addressablesLoaderService = addressablesLoaderService;
        }

        public override async Awaitable InitPool(CancellationTokenSource cancellationTokenSource)
        {
            _parentTransform = new GameObject(ParentGameObjectName).transform;
            await base.InitPool(cancellationTokenSource);
        }

        protected override async Awaitable<List<TPoolable>> CreatePoolableInstances(int instancesAmount, CancellationTokenSource cancellationTokenSource)
        {
            var poolables = new List<TPoolable>();
            var asset = await _addressablesLoaderService.LoadAsync<TPoolable>(AssetAdress, cancellationTokenSource);

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