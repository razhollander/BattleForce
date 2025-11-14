using CoreDomain.Scripts.CoreInitiator.Base;
using UnityEngine;

namespace CoreDomain.Scripts.Services.StateMachineService
{
    public interface IStateInitiator<T> where T : class, IInitiatorEnterData
    {
        Awaitable EnterState(T stateEnterData = null);
        Awaitable ExitState();
    }
}