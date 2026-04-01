using CoreDomain.Scripts.Services.Logger.Base;
using CoreDomain.Scripts.Services.UpdateService;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Core.Game.Domains.GamePlay.Presentation.Scripts.GameInputActions
{
    public class InputBeingUsedService: IUpdatable, IInputBeingUsedService
    {
        public AimInputType AimInputType { get; private set; }
        private readonly IUpdateSubscriptionService _updateSubscriptionService;
        private readonly IGameInputActionsController _gameInputActionsController;
        private Vector2 _lastMousePosition;
        private Vector2 _lastGamePadAimDirection;
        private bool _lastLeftClickState;
        private bool _lastRightClickState;

        public InputBeingUsedService(IUpdateSubscriptionService updateSubscriptionService, IGameInputActionsController gameInputActionsController)
        {
            _updateSubscriptionService = updateSubscriptionService;
            _gameInputActionsController = gameInputActionsController;
        }

        public void InitEntryPoint()
        {
            _updateSubscriptionService.RegisterUpdatable(this);
        }

        public void ManagedUpdate()
        {
            if (AimInputType == AimInputType.RightGamePad && IfMouseUsed())
            {
                AimInputType = AimInputType.Mouse;
            }
            if (AimInputType == AimInputType.Mouse && IfRightGamePadUsed())
            {
                AimInputType = AimInputType.RightGamePad;
            }
        }

        /// <summary>
        /// Checks if the mouse position or click state has changed since the last call.
        /// </summary>
        /// <returns>True if the mouse is currently being used/moved.</returns>
        private bool IfMouseUsed()
        {
            var mouse = Mouse.current;
            if (mouse == null) return false;

            // 1. Get current states
            Vector2 currentPosition = mouse.position.ReadValue();
            bool currentLeftClick = mouse.leftButton.isPressed;
            bool currentRightClick = mouse.rightButton.isPressed;

            // 2. Compare against cached states
            bool hasMoved = currentPosition != _lastMousePosition;
            bool clickChanged = (currentLeftClick != _lastLeftClickState) || 
                                (currentRightClick != _lastRightClickState);

            // 3. Update the cache for the next check
            _lastMousePosition = currentPosition;
            _lastLeftClickState = currentLeftClick;
            _lastRightClickState = currentRightClick;
            var didChange= hasMoved || clickChanged;

            if (didChange)
            {
                LogService.LogError($"Mouse used: hasMoved: {hasMoved}, clickChanged:{clickChanged}");
            }

            return didChange;
        }

        private bool IfRightGamePadUsed()
        {
            var currentAimDirection = _gameInputActionsController.GetAimDirection();
            var isZero = currentAimDirection.sqrMagnitude < 0.01f;
            if (isZero)
            {
                return false;
            }
            
            var didChange = _lastGamePadAimDirection != currentAimDirection;
            _lastGamePadAimDirection = currentAimDirection;
            return didChange;
        }

        public void ExitEntryPoint()
        {
            _updateSubscriptionService.UnregisterUpdatable(this);
        }
    }

    public enum AimInputType
    {
        Mouse,
        RightGamePad
    }
}