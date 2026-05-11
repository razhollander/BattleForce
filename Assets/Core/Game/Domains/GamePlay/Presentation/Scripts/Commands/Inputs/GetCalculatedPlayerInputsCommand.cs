using Core.Game.Domains.GamePlay.Presentation.Scripts.GameInputActions;
using Core.Game.Domains.GamePlay.Presentation.Scripts.InputBeingUsed;
using Core.Scripts.Extensions;
using CoreDomain.Scripts.Mvc.WorldCamera;
using CoreDomain.Scripts.Services.CommandFactory;
using CoreDomain.Scripts.Utils;
using UnityEngine;
using Vector2 = System.Numerics.Vector2;

namespace Core.Game.Domains.GamePlay.Presentation.Scripts.Commands.Inputs
{
    public class GetCalculatedPlayerInputsCommand : BaseCommand, ICommandWithResult<GetCalculatedPlayerInputsCommand.Result>
    {
        private IGameInputActionsController _gameInputActionsController;
        private IWorldCameraController _worldCameraController;
        private IInputBeingUsedService _inputBeingUsedService;
        
        private Vector2 _playerDirection;
        private UnityEngine.Vector2 _playerPosition;

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
             _inputBeingUsedService = _diContainer.Resolve<IInputBeingUsedService>();
        }

        public Result Execute()
        {
            var isShootInputPressed = _gameInputActionsController.IsShootInputPressed();
            var isTalentAInputPressed = _gameInputActionsController.IsTalentAInputPressed();
            var isTalentBInputPressed = _gameInputActionsController.IsTalentBInputPressed();
            var isTalentCInputPressed = _gameInputActionsController.IsTalentCInputPressed();
            var isMoveForawrdInputPressed = _gameInputActionsController.IsMoveForwardInputPressed();
            
            CalculateRightAndLeftInputs(_playerDirection, out var isMoveRightInputPressed, out var isMoveLeftInputPressed);
            var aimDirection = CalculateAimDirection();
            return new Result(isShootInputPressed, isTalentAInputPressed, isTalentBInputPressed, isTalentCInputPressed, isMoveLeftInputPressed, isMoveRightInputPressed, isMoveForawrdInputPressed, aimDirection);
        }

        private void CalculateRightAndLeftInputs(Vector2 playerDirection, out bool isMoveRightInputPressed, out bool isMoveLeftInputPressed)
        {
            if (_inputBeingUsedService.InputTypeBeingUsed == SupportedInputType.Mouse)
            {
                isMoveRightInputPressed = _gameInputActionsController.IsMoveRightInputPressed();
                isMoveLeftInputPressed = _gameInputActionsController.IsMoveLeftInputPressed();
            }
            else
            {
                var gamePadMoveDirection = _gameInputActionsController.GetMoveDirection().ToNumericsVector2();
                (isMoveRightInputPressed, isMoveLeftInputPressed) = MathUtils.GetDirectionChangeInputs(playerDirection, gamePadMoveDirection);
            }
        }

        private Vector2 CalculateAimDirection()
        {
            var mousePos = Input.mousePosition;
            var mouseWorldPos = _worldCameraController.ScreenToWorldPoint(mousePos).ToVector2XY();
            var mouseDirection = (mouseWorldPos - _playerPosition).normalized;
            var gamePadAimDirection = _gameInputActionsController.GetAimDirection();
            var aimDirection = _inputBeingUsedService.InputTypeBeingUsed == SupportedInputType.GamePad ? gamePadAimDirection.ToNumericsVector2() : mouseDirection.ToNumericsVector2();
            return aimDirection;
        }

        public struct Result
        {
            public bool IsTalentAInputPressed;
            public bool IsShootInputPressed;
            public bool IsTalentBInputPressed;
            public bool IsTalentCInputPressed;
            public bool IsMoveLeftInputPressed;
            public bool IsMoveRightInputPressed;
            public bool IsMoveForawrdInputPressed;
            public Vector2 AimDirection;

            public Result(bool isShootInputPressed, bool isTalentAInputPressed, bool isTalentBInputPressed, bool isTalentCInputPressed, bool isMoveLeftInputPressed,
                bool isMoveRightInputPressed, bool isMoveForawrdInputPressed, Vector2 aimDirection)

            {
                IsTalentAInputPressed = isTalentAInputPressed;
                IsShootInputPressed = isShootInputPressed;
                IsTalentBInputPressed = isTalentBInputPressed;
                IsTalentCInputPressed = isTalentCInputPressed;
                IsMoveLeftInputPressed = isMoveLeftInputPressed;
                IsMoveRightInputPressed = isMoveRightInputPressed;
                IsMoveForawrdInputPressed = isMoveForawrdInputPressed;
                AimDirection = aimDirection;
            }
        }
    }
}