using System.Threading;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Scripts.GameInputActions
{
    public interface IGameInputActionsController
    {
        void AddPlayer(ushort playerId, UnityEngine.InputSystem.InputDevice device);
        void EnableInputs();
        void DisableInputs();
        void RegisterAllInputListeners();
        void UnregisterAllInputListeners();
        bool IsMoveLeftInputPressed(ushort playerId);
        bool IsMoveRightInputPressed(ushort playerId);
        bool IsShootInputPressed(ushort playerId);
        bool IsMoveForwardInputPressed(ushort playerId);
        Vector2 GetAimDirection(ushort playerId);
        Vector2 GetMoveDirection(ushort playerId);
        Awaitable WaitForAnyKeyPressed(CancellationTokenSource cancellationTokenSource, bool canPressOverGui);
        bool IsTalentAInputPressed(ushort playerId);
        bool IsTalentBInputPressed(ushort playerId);
        bool IsTalentCInputPressed(ushort playerId);
    }
}