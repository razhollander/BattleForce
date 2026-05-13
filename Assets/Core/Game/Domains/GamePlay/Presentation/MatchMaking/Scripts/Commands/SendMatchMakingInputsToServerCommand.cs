using Core.Game.Domains.GamePlay.Presentation.MatchMaking.Features.Player.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.MatchMaking.Scripts.DataService;
using Core.Game.Domains.GamePlay.Presentation.Scripts.Commands.Inputs;
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
        private IFullTickPacketsHandler _fullTickPacketsHandler;
        private ITickCounterService _tickCounterService;
        private GetCalculatedPlayerInputsCommand _getCalculatedPlayerInputsCommand;
        private IMatchMakingPlayerControllers _matchMakingPlayerControllers;
        private IMatchMakingDataService _matchMakingDataService;

        private ushort _playerId;

        public SendMatchMakingInputsToServerCommand SetPlayerId(ushort playerId)
        {
            _playerId = playerId;
            return this;
        }
        
        public override void ResolveDependencies()
        {
             _clientNetworkManager = _diContainer.Resolve<IClientNetworkManager>();
             _fullTickPacketsHandler = _diContainer.Resolve<IFullTickPacketsHandler>();
             _tickCounterService = _diContainer.Resolve<ITickCounterService>();
             _matchMakingPlayerControllers = _diContainer.Resolve<IMatchMakingPlayerControllers>();
             _matchMakingDataService = _diContainer.Resolve<IMatchMakingDataService>();
             var commandFactory = _diContainer.Resolve<ICommandFactory>();
             _getCalculatedPlayerInputsCommand = commandFactory.CreateCommandWithResult<GetCalculatedPlayerInputsCommand, GetCalculatedPlayerInputsCommand.Result>();
        }

        public void Execute()
        {
            var playerPosition = _matchMakingPlayerControllers.GetPlayerPosition(_playerId);
            var playerDirection = _matchMakingDataService.GetPlayer(_playerId).Spaceship.Transform.Direction;
            var calculatedInputs = _getCalculatedPlayerInputsCommand
                .SetPlayerDirection(playerDirection)
                .SetPlayerPosition(playerPosition)
                .Execute();
            LogService.LogTopic(
                $"Sending: isMoveRightInputPressed:{calculatedInputs.IsMoveRightInputPressed},isMoveLeftInputPressed:{calculatedInputs.IsMoveLeftInputPressed},isShootInputPressed:{calculatedInputs.IsShootInputPressed}",
                LogTopicType.ClientNetwork);
            var playerInputPacket = new MatchMakingPlayerInputPacketC2S
            {
                Tick = _tickCounterService.CurrentClientTick,
                HeighestProcessedTickFromServer = _fullTickPacketsHandler.LastProcessedTickFromServer,
                IsMoveLeftInputPressed = calculatedInputs.IsMoveLeftInputPressed,
                IsMoveRightInputPressed = calculatedInputs.IsMoveRightInputPressed,
                IsShootInputPressed = calculatedInputs.IsShootInputPressed,
                IsMoveForwardInputPressed = calculatedInputs.IsMoveForawrdInputPressed
            };

            _clientNetworkManager.SendPacketSerialized(PacketTypeC2S.MatchMakingPlayerInput, playerInputPacket, DeliveryMethod.Unreliable);
        }
    }
}
