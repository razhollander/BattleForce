using System;
using Core.Game.Domains.GamePlay.Presentation.Scripts.GameInputActions;
using Core.Game.Domains.GamePlay.Presentation.Scripts.Network;
using Core.Game.Domains.GamePlay.Presentation.Scripts.Network.PacketsHandlers;
using Core.Game.Domains.GamePlay.Presentation.Scripts.TickProcessors;
using Core.Game.Domains.GamePlay.Shared.C2SModels;
using Core.Game.Domains.GamePlay.Shared.C2SModels.Packets;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.Player.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.DataService;
using Core.Scripts.Extensions;
using CoreDomain.Scripts.Mvc.WorldCamera;
using CoreDomain.Scripts.Services.CommandFactory;
using CoreDomain.Scripts.Services.Logger.Base;
using LiteNetLib;
using UnityEngine;
using Vector2 = System.Numerics.Vector2;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands
{
    public class SendMatchInputsToServerCommand : BaseCommand, ICommandVoid
    {
        private IClientNetworkManager _clientNetworkManager;
        private IGameInputActionsController _gameInputActionsController;
        private IFullTickPacketsHandler _fullTickPacketsHandler;
        private ITickCounterService _tickCounterService;
        private IMatchPlayerControllers _matchPlayerControllers;
        private IMatchDataService _matchDataService;
        private IWorldCameraController _worldCameraController;
        private IInputBeingUsedService _inputBeingUsedService;

        public override void ResolveDependencies()
        {
             _clientNetworkManager = _diContainer.Resolve<IClientNetworkManager>();
             _gameInputActionsController = _diContainer.Resolve<IGameInputActionsController>();
             _fullTickPacketsHandler = _diContainer.Resolve<IFullTickPacketsHandler>();
             _tickCounterService = _diContainer.Resolve<ITickCounterService>();
             _matchPlayerControllers = _diContainer.Resolve<IMatchPlayerControllers>();
             _matchDataService = _diContainer.Resolve<IMatchDataService>();
             _worldCameraController = _diContainer.Resolve<IWorldCameraController>();
             _inputBeingUsedService = _diContainer.Resolve<IInputBeingUsedService>();
        }

        public void Execute()
        {
            var isShootInputPressed = _gameInputActionsController.IsShootInputPressed();
            var isTalentInputPressed = _gameInputActionsController.IsTalentInputPressed();
            var isSwitchTalentInputPressed = _gameInputActionsController.IsSwitchTalentInputPressed();
            
            CalculateRightAndLeftInputs(_matchDataService.LocalPlayer.Spaceship.Transform.Direction, out var isMoveRightInputPressed, out var isMoveLeftInputPressed);
            LogService.LogTopic(
                $"Sending: isMoveRightInputPressed:{isMoveRightInputPressed},isMoveLeftInputPressed:{isMoveLeftInputPressed},isShootInputPressed:{isShootInputPressed}",
                LogTopicType.ClientNetwork);
            var playerInputPacket = new MatchPlayerInputPacketC2S
            {
                Tick = _tickCounterService.CurrentClientTick,
                HeighestProcessedTickFromServer = _fullTickPacketsHandler.LastProcessedTickFromServer,
                IsMoveLeftInputPressed = isMoveLeftInputPressed,
                IsMoveRightInputPressed = isMoveRightInputPressed,
                IsShootInputPressed = isShootInputPressed,
                IsTalentInputPressed = isTalentInputPressed,
                IsSwitchTalentInputPressed = isSwitchTalentInputPressed,
                AimDirection = CalculateAimDirection()
            };
            
            _clientNetworkManager.SendPacketSerialized(PacketTypeC2S.MatchPlayerInput, playerInputPacket, DeliveryMethod.Unreliable);
        }

        private void CalculateRightAndLeftInputs(Vector2 playerDirection, out bool isMoveRightInputPressed, out bool isMoveLeftInputPressed)
        {
            //if (_inputBeingUsedService.AimInputType == AimInputType.Mouse)
            //{
                isMoveRightInputPressed = _gameInputActionsController.IsMoveRightInputPressed();
                isMoveLeftInputPressed = _gameInputActionsController.IsMoveLeftInputPressed();
            //}
            // else
            // {
            //     var gamePadMoveDirection = _gameInputActionsController.GetMoveDirection().ToNumericsVector2();
            //     (isMoveRightInputPressed, isMoveLeftInputPressed) = MathUtils.GetDirectionChangeInputs(playerDirection, gamePadMoveDirection);
            // }
        }
        
        private Vector2 CalculateAimDirection()
        {
            var localPlayerId = _matchDataService.LocalPlayer.PlayerId;
            var playerPos = _matchPlayerControllers.GetPlayerPosition(localPlayerId);
            var mousePos = Input.mousePosition;
            var mouseWorldPos = _worldCameraController.ScreenToWorldPoint(mousePos).ToVector2XY();
            var mouseDirection = (mouseWorldPos - playerPos).normalized;
            var gamePadAimDirection = _gameInputActionsController.GetAimDirection();
            var aimDirection = _inputBeingUsedService.AimInputType == AimInputType.RightGamePad ? gamePadAimDirection.ToNumericsVector2() : mouseDirection.ToNumericsVector2();
            return aimDirection;
        }
    }
}
