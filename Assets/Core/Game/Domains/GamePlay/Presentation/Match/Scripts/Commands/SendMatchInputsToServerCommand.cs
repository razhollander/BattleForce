using Core.Game.Domains.GamePlay.Presentation.Scripts.GameInputActions;
using Core.Game.Domains.GamePlay.Presentation.Scripts.Network;
using Core.Game.Domains.GamePlay.Presentation.Scripts.Network.PacketsHandlers;
using Core.Game.Domains.GamePlay.Presentation.Scripts.TickProcessors;
using Core.Game.Domains.GamePlay.Shared.C2SModels;
using Core.Game.Domains.GamePlay.Shared.C2SModels.Packets;
using CoreDomain.Scripts.Services.CommandFactory;
using CoreDomain.Scripts.Services.Logger.Base;
using LiteNetLib;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands
{
    public class SendMatchInputsToServerCommand : BaseCommand, ICommandVoid
    {
        private IClientNetworkManager _clientNetworkManager;
        private IGameInputActionsController _gameInputActionsController;
        private ITickProcessor _tickProcessor;
        private IFullTickPacketsHandler _fullTickPacketsHandler;

        public override void ResolveDependencies()
        {
             _clientNetworkManager = _diContainer.Resolve<IClientNetworkManager>();
             _gameInputActionsController = _diContainer.Resolve<IGameInputActionsController>();
             _tickProcessor = _diContainer.Resolve<ITickProcessor>();
             _fullTickPacketsHandler = _diContainer.Resolve<IFullTickPacketsHandler>();
        }

        public void Execute()
        {
            // if (PlaybackSettings.IsPlaybackEnabled)
            // {
            //     return;
            // }

            var isMoveRightInputPressed = _gameInputActionsController.IsMoveRightInputPressed();
            var isMoveLeftInputPressed = _gameInputActionsController.IsMoveLeftInputPressed();
            var isShootInputPressed = _gameInputActionsController.IsShootInputPressed();
            var isTalentInputPressed = _gameInputActionsController.IsTalentInputPressed();
            var isSwitchTalentInputPressed = _gameInputActionsController.IsSwitchTalentInputPressed();
            LogService.LogTopic(
                $"Sending: isMoveRightInputPressed:{isMoveRightInputPressed},isMoveLeftInputPressed:{isMoveLeftInputPressed},isShootInputPressed:{isShootInputPressed}",
                LogTopicType.ClientNetwork);
            var playerInputPacket = new MatchPlayerInputPacketC2S
            {
                Tick = _tickProcessor.CurrentTick,
                HeighestProcessedTickFromServer = _fullTickPacketsHandler.LastProcessedTickFromServer,
                IsMoveLeftInputPressed = isMoveLeftInputPressed,
                IsMoveRightInputPressed = isMoveRightInputPressed,
                IsShootInputPressed = isShootInputPressed,
                IsTalentInputPressed = isTalentInputPressed,
                IsSwitchTalentInputPressed = isSwitchTalentInputPressed
            };
            
            _clientNetworkManager.SendPacketSerialized(PacketTypeC2S.MatchPlayerInput, playerInputPacket, DeliveryMethod.Unreliable);
        }
    }
}
