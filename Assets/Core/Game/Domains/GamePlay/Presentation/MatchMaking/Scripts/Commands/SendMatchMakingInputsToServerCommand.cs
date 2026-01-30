using Core.Game.Domains.GamePlay.Presentation.Scripts.GameInputActions;
using Core.Game.Domains.GamePlay.Presentation.Scripts.Network;
using Core.Game.Domains.GamePlay.Presentation.Scripts.Network.PacketsHandlers;
using Core.Game.Domains.GamePlay.Presentation.Scripts.TickProcessors;
using Core.Game.Domains.GamePlay.Shared.C2SModels;
using Core.Game.Domains.GamePlay.Shared.C2SModels.Packets;
using CoreDomain.Scripts.Services.CommandFactory;
using CoreDomain.Scripts.Services.Logger.Base;
using LiteNetLib;

namespace Core.Game.Domains.GamePlay.Presentation.MatchMaking.Scripts.Commands
{
    public class SendMatchMakingInputsToServerCommand : BaseCommand, ICommandVoid
    {
        private IClientNetworkManager _clientNetworkManager;
        private IGameInputActionsController _gameInputActionsController;
        private ITickProcessor _tickProcessor;
        private IFullTickPacketsHandler _fullTickPacketsHandler;
        private ITickCounterService _tickCounterService;

        public override void ResolveDependencies()
        {
             _clientNetworkManager = _diContainer.Resolve<IClientNetworkManager>();
             _gameInputActionsController = _diContainer.Resolve<IGameInputActionsController>();
             _tickProcessor = _diContainer.Resolve<ITickProcessor>();
             _fullTickPacketsHandler = _diContainer.Resolve<IFullTickPacketsHandler>();
             _tickCounterService = _diContainer.Resolve<ITickCounterService>();
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
            var isMoveForwardInputPressed = _gameInputActionsController.IsMoveForwardInputPressed();
            LogService.LogTopic(
                $"Sending: isMoveRightInputPressed:{isMoveRightInputPressed},isMoveLeftInputPressed:{isMoveLeftInputPressed},isShootInputPressed:{isShootInputPressed}",
                LogTopicType.ClientNetwork);
            var playerInputPacket = new MatchMakingPlayerInputPacketC2S
            {
                Tick = _tickCounterService.CurrentClientTick,
                HeighestProcessedTickFromServer = _fullTickPacketsHandler.LastProcessedTickFromServer,
                IsMoveLeftInputPressed = isMoveLeftInputPressed,
                IsMoveRightInputPressed = isMoveRightInputPressed,
                IsShootInputPressed = isShootInputPressed,
                IsMoveForwardInputPressed = isMoveForwardInputPressed,
            };
            LogService.LogError("Send matchmaking input!");
            _clientNetworkManager.SendPacketSerialized(PacketTypeC2S.MatchMakingPlayerInput, playerInputPacket, DeliveryMethod.Unreliable);
        }
    }
}
