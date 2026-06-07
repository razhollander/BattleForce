using Core.Game.Domains.GamePlay.Presentation.MatchMaking.Features.Player.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.MatchMaking.Scripts.DataService;
using Core.Game.Domains.GamePlay.Presentation.Scripts.Commands.Inputs;
using Core.Game.Domains.GamePlay.Presentation.Scripts.Network;
using Core.Game.Domains.GamePlay.Presentation.Scripts.Network.PacketsHandlers;
using Core.Game.Domains.GamePlay.Presentation.Scripts.Services.DataService;
using Core.Game.Domains.GamePlay.Presentation.Scripts.TickProcessors;
using Core.Game.Domains.GamePlay.Shared.C2SModels;
using Core.Game.Domains.GamePlay.Shared.C2SModels.Packets;
using CoreDomain.Scripts.Services.CommandFactory;
using CoreDomain.Scripts.Services.Logger.Base;
using LiteNetLib;
using Core.Scripts.Network;
using Core.Scripts.Utils.CustomCollections;

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
        private ILocalPlayersDataService _localPlayersDataService;
        
        private FixedUnorderedList<MatchMakingLocalPlayerInputDataC2S> _cachedLocalPlayersInputs;
        private MatchMakingPlayersInputPacketC2S _cahcePlayerInputPacket;

        public override void ResolveDependencies()
        {
             _clientNetworkManager = _diContainer.Resolve<IClientNetworkManager>();
             _fullTickPacketsHandler = _diContainer.Resolve<IFullTickPacketsHandler>();
             _tickCounterService = _diContainer.Resolve<ITickCounterService>();
             _matchMakingPlayerControllers = _diContainer.Resolve<IMatchMakingPlayerControllers>();
             _matchMakingDataService = _diContainer.Resolve<IMatchMakingDataService>();
             _localPlayersDataService = _diContainer.Resolve<ILocalPlayersDataService>();
             var commandFactory = _diContainer.Resolve<ICommandFactory>();
             var networkConfig = _diContainer.Resolve<NetworkConfig>();

             _getCalculatedPlayerInputsCommand = commandFactory.CreateCommandWithResult<GetCalculatedPlayerInputsCommand, GetCalculatedPlayerInputsCommand.Result>();
             _cahcePlayerInputPacket = new MatchMakingPlayersInputPacketC2S(networkConfig.MaxCap.ConcurrentPlayers);
             _cachedLocalPlayersInputs = new FixedUnorderedList<MatchMakingLocalPlayerInputDataC2S>(networkConfig.MaxCap.ConcurrentPlayers);
        }

        public void Execute()
        {
            _cachedLocalPlayersInputs.Clear();

            foreach (var playerId in _localPlayersDataService.LocalPlayerIds)
            {
                var playerPosition = _matchMakingPlayerControllers.GetPlayerPosition(playerId);
                var playerDirection = _matchMakingDataService.GetPlayer(playerId).Spaceship.Transform.Direction;
                
                var calculatedInputs = _getCalculatedPlayerInputsCommand
                    .SetPlayerId(playerId)
                    .SetPlayerDirection(playerDirection)
                    .SetPlayerPosition(playerPosition)
                    .Execute();
                    
                LogService.LogTopic(
                    $"Sending [Player {playerId}]: isMoveRightInputPressed:{calculatedInputs.IsMoveRightInputPressed},isMoveLeftInputPressed:{calculatedInputs.IsMoveLeftInputPressed},isShootInputPressed:{calculatedInputs.IsShootInputPressed}",
                    LogTopicType.ClientNetwork);
                
                ref var playerInputData = ref _cachedLocalPlayersInputs.AddAndGet();
                playerInputData.LocalPlayerId = playerId;
                playerInputData.IsMoveLeftInputPressed = calculatedInputs.IsMoveLeftInputPressed;
                playerInputData.IsMoveRightInputPressed = calculatedInputs.IsMoveRightInputPressed;
                playerInputData.IsShootInputPressed = calculatedInputs.IsShootInputPressed;
                playerInputData.IsMoveForwardInputPressed = calculatedInputs.IsMoveForawrdInputPressed;
            }

            _cahcePlayerInputPacket.Tick = _tickCounterService.CurrentClientTick;
            _cahcePlayerInputPacket.HeighestProcessedTickFromServer = _fullTickPacketsHandler.LastProcessedTickFromServer;
            _cahcePlayerInputPacket.PlayerInputs = _cachedLocalPlayersInputs;
            
            _clientNetworkManager.SendPacketSerialized(PacketTypeC2S.MatchMakingPlayersInput, _cahcePlayerInputPacket, DeliveryMethod.Unreliable);
        }
    }
}