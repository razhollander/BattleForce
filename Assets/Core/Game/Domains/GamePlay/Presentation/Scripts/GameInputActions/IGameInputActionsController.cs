using System.Threading;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Scripts.GameInputActions
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