using System.IO;
using System.Threading;
using CoreDomain.Scripts.Utils;
using UnityEngine;

namespace CoreDomain.Scripts.Services.ResourcesLoaderService
{
    public class ResourcesLoaderService : IResourcesLoaderService
    {
        public T Load<T>(string fullPath) where T : Object
        {
            return Resources.Load<T>(fullPath);
        }

        public async Awaitable<T> LoadAsync<T>(string fullPath, CancellationTokenSource cancellationTokenSource) where T : Object
        {
            cancellationTokenSource.Token.ThrowIfCancellationRequested();
            var result = await Resources.LoadAsync<T>(fullPath).WithCancellation(cancellationTokenSource.Token) as T;
            cancellationTokenSource.Token.ThrowIfCancellationRequested();
            return result;
        }

        public T LoadAndInstantiate<T>(string fullPath) where T : Component
        {
            return Object.Instantiate(Load<T>(fullPath).gameObject).GetComponent<T>();
        }

        public async Awaitable<T> LoadAndInstantiateAsync<T>(string fullPath, CancellationTokenSource cancellationTokenSource) where T : Component
        {
            cancellationTokenSource.Token.ThrowIfCancellationRequested();
            var component = await Object.InstantiateAsync(Load<T>(fullPath).gameObject);
            cancellationTokenSource.Token.ThrowIfCancellationRequested();
            return component[0].GetComponent<T>();
        }

        public T Load<T>(string path, string assetName) where T : Object
        {
            return Load<T>(Path.Combine(path, assetName));
        }
    }
}
