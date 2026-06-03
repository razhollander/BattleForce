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
        bool IsMoveLeftInputPressed(ushort playerId = 0);
        bool IsMoveRightInputPressed(ushort playerId = 0);
        bool IsShootInputPressed(ushort playerId = 0);
        bool IsMoveForwardInputPressed(ushort playerId = 0);
        Vector2 GetAimDirection(ushort playerId = 0);
        Vector2 GetMoveDirection(ushort playerId = 0);
        Awaitable WaitForAnyKeyPressed(CancellationTokenSource cancellationTokenSource, bool canPressOverGui);
        bool IsTalentAInputPressed(ushort playerId = 0);
        bool IsTalentBInputPressed(ushort playerId = 0);
        bool IsTalentCInputPressed(ushort playerId = 0);
    }
}