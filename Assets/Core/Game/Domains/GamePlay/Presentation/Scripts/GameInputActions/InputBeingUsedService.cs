using CoreDomain.Scripts.Services.UpdateService;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Core.Game.Domains.GamePlay.Presentation.Scripts.GameInputActions
{
    public class InputBeingUsedService : IUpdatable, IInputBeingUsedService
    {
        private readonly IUpdateSubscriptionService _updateSubscriptionService;
        private readonly IGameInputActionsController _gameInputActionsController;
        private Vector2 _lastMousePosition;
        private Vector2 _lastGamePadAimDirection;
        private Vector2 _lastGamePadMoveDirection;
        private bool _lastLeftClickState;
        private bool _lastRightClickState;

        public AimInputType AimInputType { get; private set; }

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
            UpdateCurrentAimInputType();
        }

        private void UpdateCurrentAimInputType()
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
            if (mouse == null)
            {
                return false;
            }

            var currentPosition = mouse.position.ReadValue();
            var currentLeftClick = mouse.leftButton.isPressed;
            var currentRightClick = mouse.rightButton.isPressed;

            var hasMouseMoved = currentPosition != _lastMousePosition;
            var hasClickChanged = (currentLeftClick != _lastLeftClickState) || 
                                (currentRightClick != _lastRightClickState);

            _lastMousePosition = currentPosition;
            _lastLeftClickState = currentLeftClick;
            _lastRightClickState = currentRightClick;
            var didChange = hasMouseMoved || hasClickChanged;
            return didChange;
        }

        private bool IfRightGamePadUsed()
        {
            var currentAimDirection = _gameInputActionsController.GetAimDirection();
            var currentMoveDirection = _gameInputActionsController.GetMoveDirection();
            var didChange = _lastGamePadAimDirection != currentAimDirection || _lastGamePadMoveDirection != currentMoveDirection;
            
            _lastGamePadAimDirection = currentAimDirection;
            _lastGamePadMoveDirection = currentMoveDirection;
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