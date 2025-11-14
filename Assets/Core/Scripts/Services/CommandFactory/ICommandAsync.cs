using System.Threading;
using UnityEngine;

namespace CoreDomain.Scripts.Services.CommandFactory
{
    public interface ICommandAsync : IBaseCommand
    {
        Awaitable Execute(CancellationTokenSource cancellationTokenSource);
    }
}