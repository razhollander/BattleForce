using Core.Game.Domains.GamePlay.Presentation.Scripts.GameInputActions;
using Core.Game.Domains.GamePlay.Presentation.Scripts.Services.DataService;
using Core.Scripts.Extensions;
using Core.Scripts.Mvc.WorldCamera;
using CoreDomain.Scripts.Services.CommandFactory;
using CoreDomain.Scripts.Utils;
using UnityEngine;
using UnityEngine.InputSystem;
using Vector2 = System.Numerics.Vector2;

namespace Core.Game.Domains.GamePlay.Presentation.Scripts.Commands.Inputs
{
    public class GetCalculatedPlayerInputsCommand : BaseCommand, ICommandWithResult<GetCalculatedPlayerInputsCommand.Result>
    {
        private IGameInputActionsController _gameInputActionsController;
        private IWorldCameraController _worldCameraController;
        private ILocalPlayersDataService _localPlayersDataService;
        private SharedGamePlayConfig _sharedGamePlayConfig;
        
        private Vector2 _playerDirection;
        private UnityEngine.Vector2 _playerPosition;
        private ushort _playerId;

        public GetCalculatedPlayerInputsCommand SetPlayerId(ushort playerId)
        {
            _playerId = playerId;
            return this;
        }

        public GetCalculatedPlayerInputsCommand SetPlayerDirection(Vector2 playerDirection)
        {
            _playerDirection = playerDirection;
            return this;
        }
        
        public GetCalculatedPlayerInputsCommand SetPlayerPosition(UnityEngine.Vector2 playerPosition)
        {
            _playerPosition = playerPosition;
            return this;
        }
        
        public override void ResolveDependencies()
        {
             _gameInputActionsController = _diContainer.Resolve<IGameInputActionsController>();
             _worldCameraController = _diContainer.Resolve<IWorldCameraController>();
             _localPlayersDataService = _diContainer.Resolve<ILocalPlayersDataService>();
             _sharedGamePlayConfig = _diContainer.Resolve<SharedGamePlayConfig>();
        }

        public Result Execute()
        {
            var isShootInputPressed = CalculateShootInput();
            var isTalentAInputPressed = _gameInputActionsController.IsPlayerTalentAInputPressed(_playerId);
            var isTalentBInputPressed = _gameInputActionsController.IsPlayerTalentBInputPressed(_playerId);
            var isTalentCInputPressed = _gameInputActionsController.IsPlayerTalentCInputPressed(_playerId);
            var isPowerUpInputPressed = _gameInputActionsController.IsPlayerPowerUpInputPressed(_playerId);
            var isMoveForawrdInputPressed = _gameInputActionsController.IsPlayerMoveForwardInputPressed(_playerId);
            var isBarrelDashInputPressed = _gameInputActionsController.IsPlayerBarrelDashInputPressed(_playerId);

            var isMoveToPointInputPressed = IsSteeringWithMouse() && _gameInputActionsController.IsPlayerMoveToPointInputPressed(_playerId);

            CalculateRightAndLeftInputs(_playerDirection, out var isMoveRightInputPressed, out var isMoveLeftInputPressed);
            var aimDirection = CalculateAimDirection(out var mouseWorldPosition, out var isUsingMouseAim);
            return new Result(isShootInputPressed, isTalentAInputPressed, isTalentBInputPressed, isTalentCInputPressed, isPowerUpInputPressed, isMoveLeftInputPressed, isMoveRightInputPressed, isMoveForawrdInputPressed, isBarrelDashInputPressed, isMoveToPointInputPressed, aimDirection, mouseWorldPosition, isUsingMouseAim);
        }

        private bool CalculateShootInput()
        {
            return IsSteeringWithMouse()
                ? _gameInputActionsController.IsPlayerShootWithMouseInputPressed(_playerId)
                : _gameInputActionsController.IsPlayerShootInputPressed(_playerId);
        }

        private bool IsSteeringWithMouse()
        {
            var isPlayerOnKeyboard = !IsPlayerOnGamepad();

            return _sharedGamePlayConfig.ShouldMoveWithMouse && isPlayerOnKeyboard;
        }

        private bool IsPlayerOnGamepad()
        {
            return _localPlayersDataService.GetInputDeviceForPlayer(_playerId) is Gamepad;
        }

        private void CalculateRightAndLeftInputs(Vector2 playerDirection, out bool isMoveRightInputPressed, out bool isMoveLeftInputPressed)
        {
            var device = _localPlayersDataService.GetInputDeviceForPlayer(_playerId);
            var isKeyboardInput = device == null || device is Keyboard || device is Mouse;
            if (isKeyboardInput)
            {
                isMoveRightInputPressed = _gameInputActionsController.IsPlayerMoveRightInputPressed(_playerId);
                isMoveLeftInputPressed = _gameInputActionsController.IsPlayerMoveLeftInputPressed(_playerId);
            }
            else
            {
                var gamePadMoveDirection = _gameInputActionsController.GetPlayerMoveDirection(_playerId).ToNumericsVector2();
                (isMoveRightInputPressed, isMoveLeftInputPressed) = MathUtils.GetDirectionChangeInputs(playerDirection, gamePadMoveDirection);
            }
        }

        private Vector2 CalculateAimDirection(out Vector2 mouseWorldPosition, out bool isUsingMouseAim)
        {
            var isGamepad = IsPlayerOnGamepad();
            isUsingMouseAim = !isGamepad;
            var mousePos = Input.mousePosition;
            var mouseWorldPos = _worldCameraController.ScreenToWorldPoint(mousePos).ToVector2XY();
            mouseWorldPosition = mouseWorldPos.ToNumericsVector2();
            var mouseDirection = (mouseWorldPos - _playerPosition).normalized;
            var gamePadAimDirection = _gameInputActionsController.GetPlayerAimDirection(_playerId);
            var aimDirection = isGamepad ? gamePadAimDirection.ToNumericsVector2() : mouseDirection.ToNumericsVector2();
            return aimDirection;
        }

        public struct Result
        {
            public bool IsTalentAInputPressed;
            public bool IsShootInputPressed;
            public bool IsTalentBInputPressed;
            public bool IsTalentCInputPressed;
            public bool IsPowerUpInputPressed;
            public bool IsMoveLeftInputPressed;
            public bool IsMoveRightInputPressed;
            public bool IsMoveForawrdInputPressed;
            public bool IsBarrelDashInputPressed;
            public bool IsMoveToPointInputPressed;
            public Vector2 AimDirection;
            public Vector2 MouseWorldPosition;
            public bool IsUsingMouseAim;

            public Result(bool isShootInputPressed, bool isTalentAInputPressed, bool isTalentBInputPressed, bool isTalentCInputPressed, bool isPowerUpInputPressed, bool isMoveLeftInputPressed,
                bool isMoveRightInputPressed, bool isMoveForawrdInputPressed, bool isBarrelDashInputPressed, bool isMoveToPointInputPressed, Vector2 aimDirection, Vector2 mouseWorldPosition, bool isUsingMouseAim)

            {
                IsTalentAInputPressed = isTalentAInputPressed;
                IsShootInputPressed = isShootInputPressed;
                IsTalentBInputPressed = isTalentBInputPressed;
                IsTalentCInputPressed = isTalentCInputPressed;
                IsPowerUpInputPressed = isPowerUpInputPressed;
                IsMoveLeftInputPressed = isMoveLeftInputPressed;
                IsMoveRightInputPressed = isMoveRightInputPressed;
                IsMoveForawrdInputPressed = isMoveForawrdInputPressed;
                IsBarrelDashInputPressed = isBarrelDashInputPressed;
                IsMoveToPointInputPressed = isMoveToPointInputPressed;
                AimDirection = aimDirection;
                MouseWorldPosition = mouseWorldPosition;
                IsUsingMouseAim = isUsingMouseAim;
            }
        }
    }
}