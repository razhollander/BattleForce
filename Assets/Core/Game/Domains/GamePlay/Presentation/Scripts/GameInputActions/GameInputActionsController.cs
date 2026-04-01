using System.Threading;
using Core.Game.Domains.GamePlay.Presentation.Scripts.Commands.Inputs;
using Core.Scripts.Utils;
using CoreDomain.Scripts.Services.CommandFactory;
using CoreDomain.Scripts.Services.Logger.Base;
using CoreDomain.Scripts.Utils;
using UnityEngine;
using UnityEngine.InputSystem;
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine.EventSystems;
#endif

namespace Core.Game.Domains.GamePlay.Presentation.Scripts.GameInputActions
{
    public class GameInputActionsController : IGameInputActionsController
    {
        private const float MIN_AIM_THRESHOLD = 0.1f;
        private const float MIN_MOVE_THRESHOLD = 0.1f;
        private readonly global::GameInputActions _gameInputActions;
        private readonly ICommandFactory _commandFactory;
        private Vector2 _lastAimDirection;

        public GameInputActionsController(
            global::GameInputActions gameInputActions,
            ICommandFactory commandFactory)
        {
            _gameInputActions = gameInputActions;
            _commandFactory = commandFactory;
        }

        public void EnableInputs()
        {
            LogService.LogTopic("EnableInputs", LogTopicType.Inputs);
            _gameInputActions.Enable();
            RegisterAllInputListeners();
        }

        public void DisableInputs()
        {
            LogService.LogTopic("DisableInputs", LogTopicType.Inputs);
            _gameInputActions.Disable();
        }
        
        public void RegisterAllInputListeners()
        {
            LogService.LogTopic("Register all input listeners", LogTopicType.Inputs);
             _gameInputActions.GamePlay.MoveRight.performed += OnShootInput;
        }

        public void UnregisterAllInputListeners()
        {
            LogService.LogTopic("Unregister all input listeners", LogTopicType.Inputs);
             _gameInputActions.GamePlay.MoveRight.performed -= OnShootInput;
        }
        
        private void OnShootInput(InputAction.CallbackContext obj)
        {
            LogService.LogTopic("Shoot input was triggered", LogTopicType.Inputs);
            _commandFactory.CreateCommandVoid<ShootInputInvokedCommand>().Execute();
        }

        public bool IsMoveLeftInputPressed()
        {
            return _gameInputActions.GamePlay.MoveLeft.IsPressed();
        }
        
        public bool IsMoveRightInputPressed()
        {
            return _gameInputActions.GamePlay.MoveRight.IsPressed();
        } 
        
        public bool IsMoveForwardInputPressed()
        {
            return _gameInputActions.GamePlay.MoveForward.IsPressed();
        }

        public Vector2 GetAimDirection()
        {
            var currentAim =  _gameInputActions.GamePlay.Aim.ReadValue<Vector2>();
            var isAimNotZero = currentAim.sqrMagnitude > MIN_AIM_THRESHOLD;
            if (isAimNotZero)
            {
                _lastAimDirection = currentAim;
            }

            return _lastAimDirection;
        }

        public Vector2 GetMoveDirection()
        {
            return _gameInputActions.GamePlay.MoveDirection.ReadValue<Vector2>();
        }

        public bool IsShootInputPressed()
        {
            return _gameInputActions.GamePlay.Shoot.IsPressed();
        }

        public bool IsTalentInputPressed()
        {
            return _gameInputActions.GamePlay.Talent.IsPressed();
        }

        public bool IsSwitchTalentInputPressed()
        {
            return _gameInputActions.GamePlay.SwitchTalent.IsPressed();
        }

        public async Awaitable WaitForAnyKeyPressed(CancellationTokenSource cancellationTokenSource, bool canPressOverGui = false)
        {
            await AwaitableUtils.WaitUntil(() => (canPressOverGui || !IsOverUiOnMobile()) && IsAnyInputPressed(),
                cancellationTokenSource.Token);
        }

        private bool IsAnyInputPressed()
        {
            return
                (Keyboard.current?.anyKey.wasPressedThisFrame == true) ||
                (Mouse.current?.leftButton.wasPressedThisFrame == true) ||
                (Mouse.current?.rightButton.wasPressedThisFrame == true) ||
                (Touchscreen.current?.primaryTouch.press.wasPressedThisFrame == true);
        }
        
        private bool IsOverUiOnMobile()
        {
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
            var isTouchPressed = Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed;
            if (!isTouchPressed)
            {
                return false;
            }
            
            Vector2 touchPosition = Touchscreen.current.primaryTouch.position.ReadValue();
            var eventDataCurrentPosition = new PointerEventData(EventSystem.current);
            eventDataCurrentPosition.position = new Vector2(touchPosition.x, touchPosition.y);
            var results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
            return results.Count > 0;
#endif
            return false;
        }
    }
}