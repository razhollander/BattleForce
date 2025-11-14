using System.Threading;
using UnityEngine;

namespace CoreDomain.Scripts.Services.CommandFactory
{
    public interface ICommandAsyncWithResult<TReturn> : IBaseCommand
    {
        Awaitable<TReturn> Execute(CancellationTokenSource cancellationTokenSource = null);
    }
}