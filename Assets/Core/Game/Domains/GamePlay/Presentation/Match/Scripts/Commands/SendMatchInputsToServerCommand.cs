using System.Collections.Generic;
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
using Core.Game.Domains.GamePlay.Presentation.Scripts.Services.DataService;
using Core.Scripts.Network;
using Core.Scripts.Utils.CustomCollections;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands
{
    public class SendMatchInputsToServerCommand : BaseCommand, ICommandVoid
    {
        private IClientNetworkManager _clientNetworkManager;
        private IFullTickPacketsHandler _fullTickPacketsHandler;
        private ITickCounterService _tickCounterService;
        private IMatchPlayerControllers _matchPlayerControllers;
        private IMatchDataService _matchDataService;
        private ILocalPlayersDataService _localPlayersDataService;
        private GetCalculatedPlayerInputsCommand _getCalculatedPlayerInputsCommand;
        private FixedUnorderedList<MatchLocalPlayerInputDataC2S> _cachedLocalPlayersInputs;
        private MatchPlayersInputPacketC2S _cachedPlayerInputPacket;

        public override void ResolveDependencies()
        {
             _clientNetworkManager = _diContainer.Resolve<IClientNetworkManager>();
             _fullTickPacketsHandler = _diContainer.Resolve<IFullTickPacketsHandler>();
             _tickCounterService = _diContainer.Resolve<ITickCounterService>();
             _matchPlayerControllers = _diContainer.Resolve<IMatchPlayerControllers>();
             _matchDataService = _diContainer.Resolve<IMatchDataService>();
             _localPlayersDataService = _diContainer.Resolve<ILocalPlayersDataService>();
             var commandFactory = _diContainer.Resolve<ICommandFactory>();
             var networkConfig = _diContainer.Resolve<NetworkConfig>();
             _getCalculatedPlayerInputsCommand = commandFactory.CreateCommandWithResult<GetCalculatedPlayerInputsCommand, GetCalculatedPlayerInputsCommand.Result>();
             _cachedLocalPlayersInputs = new FixedUnorderedList<MatchLocalPlayerInputDataC2S>(networkConfig.MaxCap.ConcurrentPlayers);
             _cachedPlayerInputPacket = new MatchPlayersInputPacketC2S(networkConfig.MaxCap.ConcurrentPlayers);
        }

        public void Execute()
        {
            _cachedLocalPlayersInputs.Clear();

            foreach (var playerId in _localPlayersDataService.LocalPlayersIds)
            {
                var playerPosition = _matchPlayerControllers.GetPlayerPosition(playerId);
                var playerDirection = _matchDataService.GetPlayer(playerId).Spaceship.Transform.Direction;

                var calculatedInputs = _getCalculatedPlayerInputsCommand
                    .SetPlayerDirection(playerDirection)
                    .SetPlayerId(playerId)
                    .SetPlayerPosition(playerPosition)
                    .Execute();

                LogService.LogTopic(
                    $"Sending: isMoveRightInputPressed:{calculatedInputs.IsMoveRightInputPressed},isMoveLeftInputPressed:{calculatedInputs.IsMoveLeftInputPressed},isShootInputPressed:{calculatedInputs.IsShootInputPressed}",
                    LogTopicType.ClientNetwork);

                ref var playerInputDataC2S = ref _cachedLocalPlayersInputs.AddAndGet();
                playerInputDataC2S.PlayerId = playerId;
                playerInputDataC2S.IsMoveLeftInputPressed = calculatedInputs.IsMoveLeftInputPressed;
                playerInputDataC2S.IsMoveRightInputPressed = calculatedInputs.IsMoveRightInputPressed;
                playerInputDataC2S.IsShootInputPressed = calculatedInputs.IsShootInputPressed;
                playerInputDataC2S.IsTalentAInputPressed = calculatedInputs.IsTalentAInputPressed;
                playerInputDataC2S.IsTalentBInputPressed = calculatedInputs.IsTalentBInputPressed;
                playerInputDataC2S.IsTalentCInputPressed = calculatedInputs.IsTalentCInputPressed;
                playerInputDataC2S.IsPowerUpInputPressed = calculatedInputs.IsPowerUpInputPressed;
                playerInputDataC2S.IsBarrelDashInputPressed = calculatedInputs.IsBarrelDashInputPressed;
                playerInputDataC2S.IsMoveToPointInputPressed = calculatedInputs.IsMoveToPointInputPressed;
                playerInputDataC2S.AimDirection = calculatedInputs.AimDirection;
                playerInputDataC2S.IsUsingMouseAim = calculatedInputs.IsUsingMouseAim;
                playerInputDataC2S.MouseWorldPosition = calculatedInputs.MouseWorldPosition;
            }

            _cachedPlayerInputPacket.Tick = _tickCounterService.CurrentClientTick;
            _cachedPlayerInputPacket.HeighestProcessedTickFromServer = _fullTickPacketsHandler.LastProcessedTickFromServer;
            _cachedPlayerInputPacket.PlayerInputs = _cachedLocalPlayersInputs;

            _clientNetworkManager.SendPacketSerialized(PacketTypeC2S.MatchPlayersInput, _cachedPlayerInputPacket, DeliveryMethod.Unreliable);
        }
    }
}
