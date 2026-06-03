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
using Core.Game.Domains.GamePlay.Presentation.Scripts.Commands.Inputs;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands
{
    public class SendMatchInputsToServerCommand : BaseCommand, ICommandVoid
    {
        private IClientNetworkManager _clientNetworkManager;
        private IFullTickPacketsHandler _fullTickPacketsHandler;
        private ITickCounterService _tickCounterService;
        private IMatchPlayerControllers _matchPlayerControllers;
        private IMatchDataService _matchDataService;
        private GetCalculatedPlayerInputsCommand _getCalculatedPlayerInputsCommand;
        
        private ushort _playerId;

        public SendMatchInputsToServerCommand SetPlayerId(ushort playerId)
        {
            _playerId = playerId;
            return this;
        }
        
        public override void ResolveDependencies()
        {
             _clientNetworkManager = _diContainer.Resolve<IClientNetworkManager>();
             _fullTickPacketsHandler = _diContainer.Resolve<IFullTickPacketsHandler>();
             _tickCounterService = _diContainer.Resolve<ITickCounterService>();
             _matchPlayerControllers = _diContainer.Resolve<IMatchPlayerControllers>();
             _matchDataService = _diContainer.Resolve<IMatchDataService>();
             var commandFactory = _diContainer.Resolve<ICommandFactory>();
             _getCalculatedPlayerInputsCommand = commandFactory.CreateCommandWithResult<GetCalculatedPlayerInputsCommand, GetCalculatedPlayerInputsCommand.Result>();
        }

        public void Execute()
        {
          var playerPosition = _matchPlayerControllers.GetPlayerPosition(_playerId);
            var playerDirection = _matchDataService.GetPlayer(_playerId).Spaceship.Transform.Direction;
            var calculatedInputs = _getCalculatedPlayerInputsCommand
                .SetPlayerId(_playerId)
                .SetPlayerDirection(playerDirection)
                .SetPlayerPosition(playerPosition)
                .Execute();
            LogService.LogTopic(
                $"Sending: isMoveRightInputPressed:{calculatedInputs.IsMoveRightInputPressed},isMoveLeftInputPressed:{calculatedInputs.IsMoveLeftInputPressed},isShootInputPressed:{calculatedInputs.IsShootInputPressed}",
                LogTopicType.ClientNetwork);
            var playerInputPacket = new MatchPlayerInputPacketC2S
            {
                Tick = _tickCounterService.CurrentClientTick,
                HeighestProcessedTickFromServer = _fullTickPacketsHandler.LastProcessedTickFromServer,
                IsMoveLeftInputPressed = calculatedInputs.IsMoveLeftInputPressed,
                IsMoveRightInputPressed = calculatedInputs.IsMoveRightInputPressed,
                IsShootInputPressed = calculatedInputs.IsShootInputPressed,
                IsTalentAInputPressed = calculatedInputs.IsTalentAInputPressed,
                IsTalentBInputPressed = calculatedInputs.IsTalentBInputPressed,
                IsTalentCInputPressed = calculatedInputs.IsTalentCInputPressed,
                AimDirection = calculatedInputs.AimDirection
            };
            
            _clientNetworkManager.SendPacketSerialized(PacketTypeC2S.MatchPlayerInput, playerInputPacket, DeliveryMethod.Unreliable);
        }
    }
}
