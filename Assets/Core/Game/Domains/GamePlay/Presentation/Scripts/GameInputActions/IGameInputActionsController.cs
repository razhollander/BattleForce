using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Core.Game.Domains.GamePlay.Presentation.Scripts.GameInputActions
{
    public interface IGameInputActionsController
    {
        void AddPlayer(ushort playerId, InputDevice device);
        void AddPlayerIfNotAlreadyExist(ushort playerId, InputDevice device);
        void EnableInputs();
        void DisableInputs();
        void RegisterAllInputListeners();
        void UnregisterAllInputListeners();
        bool IsPlayerMoveLeftInputPressed(ushort playerId);
        bool IsPlayerMoveRightInputPressed(ushort playerId);
        bool IsPlayerShootInputPressed(ushort playerId);
        bool IsPlayerMoveForwardInputPressed(ushort playerId);
        Vector2 GetPlayerAimDirection(ushort playerId);
        Vector2 GetPlayerMoveDirection(ushort playerId);
        Awaitable WaitForAnyKeyPressed(CancellationTokenSource cancellationTokenSource, bool canPressOverGui);
        bool IsPlayerTalentAInputPressed(ushort playerId);
        bool IsPlayerTalentBInputPressed(ushort playerId);
        bool IsPlayerTalentCInputPressed(ushort playerId);
        bool IsPlayerPowerUpInputPressed(ushort playerId);
    }
}