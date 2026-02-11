using Core.Game.Domains.GamePlay.Presentation.Scripts.GameInputActions;
using Core.Game.Domains.GamePlay.Presentation.Scripts.Network;
using Core.Game.Domains.GamePlay.Presentation.Scripts.Network.PacketsHandlers;
using Core.Game.Domains.GamePlay.Presentation.Scripts.TickProcessors;
using Core.Game.Domains.GamePlay.Shared.C2SModels;
using Core.Game.Domains.GamePlay.Shared.C2SModels.Packets;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.Player.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.DataService;
using CoreDomain.Scripts.Services.CommandFactory;
using CoreDomain.Scripts.Services.Logger.Base;
using LiteNetLib;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands
{
    public class SendMatchInputsToServerCommand : BaseCommand, ICommandVoid
    {
        private IClientNetworkManager _clientNetworkManager;
        private IGameInputActionsController _gameInputActionsController;
        private ITickProcessor _tickProcessor;
        private IFullTickPacketsHandler _fullTickPacketsHandler;
        private ITickCounterService _tickCounterService;
        private IMatchPlayerControllers _matchPlayerControllers;
        private IMatchDataService _matchDataService;

        public override void ResolveDependencies()
        {
             _clientNetworkManager = _diContainer.Resolve<IClientNetworkManager>();
             _gameInputActionsController = _diContainer.Resolve<IGameInputActionsController>();
             _tickProcessor = _diContainer.Resolve<ITickProcessor>();
             _fullTickPacketsHandler = _diContainer.Resolve<IFullTickPacketsHandler>();
             _tickCounterService = _diContainer.Resolve<ITickCounterService>();
             _matchPlayerControllers = _diContainer.Resolve<IMatchPlayerControllers>();
             _matchDataService = _diContainer.Resolve<IMatchDataService>();
        }

        public void Execute()
        {
            var isMoveRightInputPressed = _gameInputActionsController.IsMoveRightInputPressed();
            var isMoveLeftInputPressed = _gameInputActionsController.IsMoveLeftInputPressed();
            var isShootInputPressed = _gameInputActionsController.IsShootInputPressed();
            var isTalentInputPressed = _gameInputActionsController.IsTalentInputPressed();
            var isSwitchTalentInputPressed = _gameInputActionsController.IsSwitchTalentInputPressed();
            LogService.LogTopic(
                $"Sending: isMoveRightInputPressed:{isMoveRightInputPressed},isMoveLeftInputPressed:{isMoveLeftInputPressed},isShootInputPressed:{isShootInputPressed}",
                LogTopicType.ClientNetwork);

            var aimDirection = System.Numerics.Vector2.Zero;

            if (_matchDataService.LocalPlayer != null)
            {
                var localPlayerId = _matchDataService.LocalPlayer.PlayerId;
                var playerTransform = _matchPlayerControllers.GetPlayerTransform(localPlayerId);
                var mousePos = Input.mousePosition;
                var mouseWorldPos = Camera.main.ScreenToWorldPoint(mousePos);
                mouseWorldPos.z = 0;
                var playerPos = playerTransform.position;
                var direction = (mouseWorldPos - playerPos).normalized;
                aimDirection = new System.Numerics.Vector2(direction.x, direction.y);
            }

            var playerInputPacket = new MatchPlayerInputPacketC2S
            {
                Tick = _tickCounterService.CurrentClientTick,
                HeighestProcessedTickFromServer = _fullTickPacketsHandler.LastProcessedTickFromServer,
                IsMoveLeftInputPressed = isMoveLeftInputPressed,
                IsMoveRightInputPressed = isMoveRightInputPressed,
                IsShootInputPressed = isShootInputPressed,
                IsTalentInputPressed = isTalentInputPressed,
                IsSwitchTalentInputPressed = isSwitchTalentInputPressed,
                AimDirection = aimDirection
            };
            
            _clientNetworkManager.SendPacketSerialized(PacketTypeC2S.MatchPlayerInput, playerInputPacket, DeliveryMethod.Unreliable);
        }
    }
}
