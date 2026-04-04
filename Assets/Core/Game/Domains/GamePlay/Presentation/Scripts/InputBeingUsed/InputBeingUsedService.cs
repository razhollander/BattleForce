using Core.Game.Domains.GamePlay.Presentation.Scripts.GameInputActions;
using CoreDomain.Scripts.Services.UpdateService;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Core.Game.Domains.GamePlay.Presentation.Scripts.InputBeingUsed
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

        public SupportedInputType InputTypeBeingUsed { get; private set; }

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
            var shouldChangeToMouseInput = InputTypeBeingUsed == SupportedInputType.GamePad && IsMouseCurrentlyUsed();
            if (shouldChangeToMouseInput)
            {
                InputTypeBeingUsed = SupportedInputType.Mouse;
            }

            var shouldChangeToGamePadInput = InputTypeBeingUsed == SupportedInputType.Mouse && IsGamePadCurrentlyUsed();
            if (shouldChangeToGamePadInput)
            {
                InputTypeBeingUsed = SupportedInputType.GamePad;
            }
        }
        
        private bool IsMouseCurrentlyUsed()
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
            var didAnyMouseInputChange = hasMouseMoved || hasClickChanged;
            return didAnyMouseInputChange;
        }

        private bool IsGamePadCurrentlyUsed()
        {
            var currentAimDirection = _gameInputActionsController.GetAimDirection();
            var currentMoveDirection = _gameInputActionsController.GetMoveDirection();
            var didChange = _lastGamePadAimDirection != currentAimDirection || _lastGamePadMoveDirection != currentMoveDirection;
            
            _lastGamePadAimDirection = currentAimDirection;
            _lastGamePadMoveDirection = currentMoveDirection;
            return didChange;
        }

        public void InitExitPoint()
        {
            _updateSubscriptionService.UnregisterUpdatable(this);
        }
    }
}