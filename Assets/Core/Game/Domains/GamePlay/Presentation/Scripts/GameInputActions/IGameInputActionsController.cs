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
        bool IsMoveLeftInputPressed();
        bool IsMoveRightInputPressed();
        bool IsShootInputPressed();
        bool IsMoveForwardInputPressed();
        Vector2 GetAimDirection();
        Awaitable WaitForAnyKeyPressed(CancellationTokenSource cancellationTokenSource, bool canPressOverGui);
        bool IsTalentInputPressed();
        bool IsSwitchTalentInputPressed();
    }
}