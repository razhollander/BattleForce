using System.Threading;
using CoreDomain.GameDomain.GameStateDomain.GamePlayDomain.Scripts.Commands;
using CoreDomain.Scripts.Services.CommandFactory;
using CoreDomain.Scripts.Services.Logger.Base;
using CoreDomain.Scripts.Utils;
using UnityEngine;
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine.EventSystems;
#endif
using UnityEngine.InputSystem;

namespace CoreDomain.GameDomain.GameStateDomain.GamePlayDomain.Scripts.Mvc.GameInputActions
{
    public class GameInputActionsController : IGameInputActionsController
    {
        private readonly global::GameInputActions _gameInputActions;
        private readonly ICommandFactory _commandFactory;

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
        }

        public void DisableInputs()
        {
            LogService.LogTopic("DisableInputs", LogTopicType.Inputs);
            _gameInputActions.Disable();
        }
        
        public void RegisterAllInputListeners()
        {
            LogService.LogTopic("Register all input listeners", LogTopicType.Inputs);
            _gameInputActions.GamePlay.Jump.started += OnJumpInput;
            _gameInputActions.GamePlay.Shoot.performed += OnShootInput;
        }
        
        public void UnregisterAllInputListeners()
        {
            LogService.LogTopic("Unregister all input listeners", LogTopicType.Inputs);
            _gameInputActions.GamePlay.Jump.started -= OnJumpInput;
            _gameInputActions.GamePlay.Shoot.performed -= OnShootInput;
        }
        
        private void OnShootInput(InputAction.CallbackContext obj)
        {
            if (IsOverUiOnMobile())
            {
                return;
            }

            LogService.LogTopic("Shoot input was triggered", LogTopicType.Inputs);
            //_commandFactory.CreateCommandVoid<ShootInputInvokedCommand>().Execute();
        }

        public bool IsJumpInputPressed()
        {
            return !IsOverUiOnMobile() && _gameInputActions.GamePlay.Jump.IsPressed();
        }

        private void OnJumpInput(InputAction.CallbackContext context)
        {
            if (IsOverUiOnMobile())
            {
                return;
            }
            
            LogService.LogTopic("Jump input was triggered", LogTopicType.Inputs);
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