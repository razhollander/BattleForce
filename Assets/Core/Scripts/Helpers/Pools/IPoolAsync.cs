using System.Threading;
using UnityEngine;

namespace CoreDomain.Scripts.Helpers.Pools
{
    public interface IPoolAsync<T> where T : IPoolable
    {
        Awaitable InitPool(CancellationTokenSource cancellationTokenSource);
        Awaitable<T> Spawn(CancellationTokenSource cancellationTokenSource);
    }
}