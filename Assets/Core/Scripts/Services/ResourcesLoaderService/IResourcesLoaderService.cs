using System.Threading;
using UnityEngine;

namespace CoreDomain.Scripts.Services.ResourcesLoaderService
{
    public interface IResourcesLoaderService
    {
        T Load<T>(string fullPath) where T : Object;
        Awaitable<T> LoadAsync<T>(string fullPath, CancellationTokenSource cancellationTokenSource) where T : Object;
        T LoadAndInstantiate<T>(string fullPath) where T : Component;
        Awaitable<T> LoadAndInstantiateAsync<T>(string fullPath, CancellationTokenSource cancellationTokenSource) where T : Component;
        T Load<T>(string path, string assetName) where T : Object;
    }
}