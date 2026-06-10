using System.Collections.Generic;
using System.Threading;
using Core.Game.Domains.GamePlay.Presentation.Scripts.Commands.Inputs;
using Core.Scripts.Utils;
using CoreDomain.Scripts.Services.CommandFactory;
using CoreDomain.Scripts.Services.Logger.Base;
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

        private readonly Dictionary<ushort, global::GameInputActions> _gameInputActionsByPlayer = new Dictionary<ushort, global::GameInputActions>();
        private global::GameInputActions _defaultGameInputActions;
        private readonly ICommandFactory _commandFactory;
        private Dictionary<ushort, Vector2> _lastAimDirectionByPlayer = new Dictionary<ushort, Vector2>();

        public GameInputActionsController(
            global::GameInputActions gameInputActions,
            ICommandFactory commandFactory)
        {
            _defaultGameInputActions = gameInputActions;
            _gameInputActionsByPlayer[0] = gameInputActions;
            _commandFactory = commandFactory;
        }
        
        public void AddPlayer(ushort playerId, InputDevice device)
        {
            if (_gameInputActionsByPlayer.ContainsKey(playerId))
            {
                LogService.LogError("Player with id " + playerId + " already exists in GameInputActionsController");
            }
            else
            {
                var inputActions = new global::GameInputActions();

                if (device != null)
                {
                    inputActions.devices = new UnityEngine.InputSystem.Utilities.ReadOnlyArray<InputDevice>(new[] {device});
                }

                inputActions.Enable();
                inputActions.GamePlay.MoveRight.performed += OnShootInput;
                _gameInputActionsByPlayer[playerId] = inputActions;
            }
        }

        public void EnableInputs()
        {
            LogService.LogTopic("EnableInputs", LogTopicType.Inputs);
            foreach (var actions in _gameInputActionsByPlayer.Values)
            {
                actions.Enable();
            }
            RegisterAllInputListeners();
        }

        public void DisableInputs()
        {
            LogService.LogTopic("DisableInputs", LogTopicType.Inputs);
            foreach (var actions in _gameInputActionsByPlayer.Values)
            {
                actions.Disable();
                actions.GamePlay.Disable();
                actions.UI.Disable();
            }
        }

        public void RegisterAllInputListeners()
        {
            LogService.LogTopic("Register all input listeners", LogTopicType.Inputs);
             foreach (var actions in _gameInputActionsByPlayer.Values)
            {
                actions.GamePlay.MoveRight.performed += OnShootInput;
            }
        }

        public void UnregisterAllInputListeners()
        {
            LogService.LogTopic("Unregister all input listeners", LogTopicType.Inputs);
             foreach (var actions in _gameInputActionsByPlayer.Values)
            {
                actions.GamePlay.MoveRight.performed -= OnShootInput;
            }
        }

        private void OnShootInput(InputAction.CallbackContext obj)
        {
            LogService.LogTopic("Shoot input was triggered", LogTopicType.Inputs);
            _commandFactory.CreateCommandVoid<ShootInputInvokedCommand>().Execute();
        }

        public bool IsMoveLeftInputPressed(ushort playerId = 0)
        {
            if (!TryGetPlayerInputActions(playerId, out var actions))
            {
                return false; 
            }
            
            return actions.GamePlay.MoveLeft.IsPressed();
        }

        public bool IsMoveRightInputPressed(ushort playerId)
        {
            if (!TryGetPlayerInputActions(playerId, out var actions))
            {
                return false; 
            }
                
            return actions.GamePlay.MoveRight.IsPressed();
        }

        public bool IsMoveForwardInputPressed(ushort playerId)
        {
            if (!TryGetPlayerInputActions(playerId, out var actions))
            {
                return false; 
            }
            
            return actions.GamePlay.MoveForward.IsPressed();
        }

        public Vector2 GetAimDirection(ushort playerId)
        {
            if (!TryGetPlayerInputActions(playerId, out var actions))
            {
                return Vector2.zero;
            }

            var currentAim = actions.GamePlay.Aim.ReadValue<Vector2>();
            var isAimNotZero = currentAim.sqrMagnitude > MIN_AIM_THRESHOLD;
            if (isAimNotZero)
            {
                _lastAimDirectionByPlayer[playerId] = currentAim;
            }

            _lastAimDirectionByPlayer.TryGetValue(playerId, out var lastAim);
            return lastAim;
        }

        public Vector2 GetMoveDirection(ushort playerId)
        {
            if (!TryGetPlayerInputActions(playerId, out var actions))
            {
                return Vector2.zero;
            }
            
            return actions.GamePlay.MoveDirection.ReadValue<Vector2>();
        }

        private bool TryGetPlayerInputActions(ushort playerId, out global::GameInputActions inputActions)
        {
            if (!_gameInputActionsByPlayer.TryGetValue(playerId, out inputActions))
            {
                LogService.LogError($"Player with id {playerId} is not registered in GameInputActionsController");

                return false;
            }

            return true;
        }
        
        public bool IsShootInputPressed(ushort playerId)
        {
            if (!TryGetPlayerInputActions(playerId, out var actions))
            {
                return false;
            }
            
            return actions.GamePlay.Shoot.IsPressed();
        }

        public bool IsTalentAInputPressed(ushort playerId)
        {
            if (!TryGetPlayerInputActions(playerId, out var actions))
            {
                return false;
            }
            
            return actions.GamePlay.TalentA.IsPressed();
        }
        public bool IsTalentBInputPressed(ushort playerId)
        {
            if (!TryGetPlayerInputActions(playerId, out var actions))
            {
                return false;
            }
            
            return actions.GamePlay.TalentB.IsPressed();
        }
        public bool IsTalentCInputPressed(ushort playerId)
        {
            if (!TryGetPlayerInputActions(playerId, out var actions))
            {
                return false;
            }
            
            return actions.GamePlay.TalentC.IsPressed();
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