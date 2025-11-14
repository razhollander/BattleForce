using System.Collections.Generic;
using CoreDomain.Scripts.Services.AssetBundleLoaderService;
using UnityEngine;
using Zenject;

namespace CoreDomain.Scripts.Helpers.Pools
{
    public abstract class AssetFromBundlePool<TPoolable> : BasePool<TPoolable>, IAssetFromBundlePool<TPoolable> where TPoolable : MonoBehaviour, IPoolable
    {
        private readonly DiContainer _diContainer;
        private readonly IAssetBundleLoaderService _assetBundleLoaderService;
        protected abstract string AssetBundlePathName { get; }
        public abstract string AssetName { get; }
        protected abstract string ParentGameObjectName { get; }
        private Transform _parentTransform;

        public AssetFromBundlePool(PoolData poolData, DiContainer diContainer, IAssetBundleLoaderService assetBundleLoaderService) : base(poolData)
        {
            _diContainer = diContainer;
            _assetBundleLoaderService = assetBundleLoaderService;
        }

        public override void InitPool()
        {
            _parentTransform = new GameObject(ParentGameObjectName).transform;
            base.InitPool();
        }

        protected override List<TPoolable> CreatePoolableInstances(int instancesAmount)
        {
            var poolables = new List<TPoolable>();
            var bundle = _assetBundleLoaderService.LoadAssetBundle(AssetBundlePathName);

            for (int i = 0; i < instancesAmount; i++)
            {
                var poolable = _diContainer.InstantiatePrefab(_assetBundleLoaderService.LoadAssetFromBundle<GameObject>(bundle, AssetName));
                poolable.SetActive(false);
                poolable.transform.SetParent(_parentTransform);
                poolables.Add(poolable.GetComponent<TPoolable>());
            }

            _assetBundleLoaderService.UnloadAssetBundle(bundle);

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