using System.Collections.Generic;
using System.Threading;
using CoreDomain.Scripts.Services.Logger.Base;
using CoreDomain.Scripts.Utils;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace CoreDomain.Scripts.Services.AddressablesLoader
{
    public class AddressablesLoaderService : IAddressablesLoaderService
    {
        private readonly Dictionary<string, AsyncOperationHandle> _cachedHandlesPerAddress = new();

        public async Awaitable<T> LoadAsync<T>(string address, CancellationTokenSource cancellationTokenSource) where T : Object
        {
            if (_cachedHandlesPerAddress.TryGetValue(address, out var cachedHandle))
            {
                return TryGetComponent<T>(address, (Object)cachedHandle.Result);
            }

            var handle = Addressables.LoadAssetAsync<Object>(address);
            _cachedHandlesPerAddress[address] = handle;

            await handle.WithCancellation(cancellationTokenSource.Token);
            cancellationTokenSource.Token.ThrowIfCancellationRequested();

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                return TryGetComponent<T>(address, handle.Result);
            }

            LogService.LogError($"Failed to load asset at address: {address}. Reason: {handle.OperationException?.Message}");
            _cachedHandlesPerAddress.Remove(address);
            return null;
        }

        public void Release(string address)
        {
            if (_cachedHandlesPerAddress.TryGetValue(address, out var handle))
            {
                Addressables.Release(handle);
                _cachedHandlesPerAddress.Remove(address);
            }
        }

        public void ReleaseAll()
        {
            foreach (var handle in _cachedHandlesPerAddress.Values)
            {
                Addressables.Release(handle);
            }
            _cachedHandlesPerAddress.Clear();
        }

        public bool IsLoaded(string address)
        {
            return _cachedHandlesPerAddress.ContainsKey(address);
        }

        private T TryGetComponent<T>(string address, Object loadedAsset) where T : Object
        {
            if (typeof(MonoBehaviour).IsAssignableFrom(typeof(T)) && loadedAsset is GameObject go)
            {
                var component = go.GetComponent<T>();
                if (component != null)
                {
                    return component;
                }

                LogService.LogError($"GameObject at address '{address}' does not have a component of type {typeof(T).Name}");
                return null;
            }

            return loadedAsset as T;
        }
    }
}
