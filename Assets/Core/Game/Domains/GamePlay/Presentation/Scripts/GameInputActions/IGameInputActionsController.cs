using System.Threading;
using UnityEngine;

namespace CoreDomain.GameDomain.GameStateDomain.GamePlayDomain.Scripts.Mvc.GameInputActions
{
    public interface IGameInputActionsController
    {
        void EnableInputs();
        void DisableInputs();
        void RegisterAllInputListeners();
        void UnregisterAllInputListeners();
        bool IsMovementInputPressed();
        Awaitable WaitForAnyKeyPressed(CancellationTokenSource cancellationTokenSource, bool canPressOverGui);
    }
}