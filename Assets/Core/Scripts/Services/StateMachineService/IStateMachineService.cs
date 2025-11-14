
using System.Threading;
using UnityEngine;

namespace CoreDomain.Scripts.Services.StateMachineService
{
    public interface IStateMachineService
    {
        IGameState CurrentState();
        Awaitable EnterInitialGameState(IGameState initialState ,CancellationTokenSource cancellationTokenSource);
        void SwitchState(IGameState newState);
    }
}