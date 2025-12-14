using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Presentation.Features.Player.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Scripts.Commands.NetEventsCommands;
using Core.Game.Domains.GamePlay.Presentation.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Presentation.Scripts.Presentation;
using Core.Game.Domains.GamePlay.Shared;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Shared.S2CModels.PacketEvents;
using Core.Scripts.Network;
using CoreDomain.Scripts.Extensions;
using CoreDomain.Scripts.Services.CommandFactory;
using CoreDomain.Scripts.Services.Logger.Base;

namespace Core.Game.Domains.GamePlay.Presentation.Scripts.Network.PacketsHandlers
{
    public class SimulationNetEventsHandler
    {
        private readonly IMatchDataService _matchDataService;
        private readonly IMatchNetEventsDataService _matchNetEventsDataService;
        private readonly IClientNetworkManager _networkManager;
        private readonly IPlayerControllers _playerControllers;
        private readonly NetworkConfig _networkConfig;
        private readonly IClientPresentationTickProcessor _clientPresentationTickProcessor;
        private readonly ICommandFactory _commandFactory;

        public SimulationNetEventsHandler(IMatchDataService matchDataService,
            IMatchNetEventsDataService matchNetEventsDataService, IClientNetworkManager networkManager,
            IPlayerControllers playerControllers, NetworkConfig networkConfig,
            IClientPresentationTickProcessor clientPresentationTickProcessor, ICommandFactory commandFactory)
        {
            _matchDataService = matchDataService;
            _matchNetEventsDataService = matchNetEventsDataService;
            _networkManager = networkManager;
            _playerControllers = playerControllers;
            _networkConfig = networkConfig;
            _clientPresentationTickProcessor = clientPresentationTickProcessor;
            _commandFactory = commandFactory;
        }

        public void ProcessPlayerJoinedEvents(List<PlayerJoinAcceptPacketS2C> playerJoinAcceptNetEvents, ref int clientTick)
        {
            foreach (var playerJoinAcceptNetEvent in playerJoinAcceptNetEvents)
            {
                var playerId = playerJoinAcceptNetEvent.PlayerState.Id;
                var isLocalPlayer = playerJoinAcceptNetEvent.IsLocal;
                LogService.LogTopic(
                    $"Join packet accepted processed,  isLocalPlayer:{isLocalPlayer}, player id: " + playerId,
                    LogTopicType.ClientNetwork);
                
                if (isLocalPlayer)
                {
                    _commandFactory.CreateCommandVoid<SyncSimulationStateCommand>()
                        .SetSimulationState(playerJoinAcceptNetEvent.SimulationState).Execute();
                    SyncTickToServer(out clientTick, playerJoinAcceptNetEvent);
                    SetupLocalPlayer(playerId);
                }
                else
                {
                    var playerModel = _matchDataService.AddPlayer(playerJoinAcceptNetEvent.PlayerState);
                    _playerControllers.CreatePlayer(playerModel.PlayerId);
                }
            }
        }

        private void SyncTickToServer(out int clientTick, PlayerJoinAcceptPacketS2C playerJoinAcceptNetEvent)
        {
            var ticksPassedSinceServerSendPacket = (_networkManager.Ping / 1000f) / _networkConfig.DeltaTime;
            var tickWouldBeOnServerWhenReceiveMyPackets = (int)(ticksPassedSinceServerSendPacket * 2) + playerJoinAcceptNetEvent.OccuredOnTick;
            clientTick = tickWouldBeOnServerWhenReceiveMyPackets;
        }

        private void SetupLocalPlayer(int playerId)
        {
            _matchDataService.SetLocalPlayer(playerId);
            _clientPresentationTickProcessor.StartTick();
        }
        
        public void ProcessBulletSpawnEvents(List<BulletSpawnNetEventS2C> bulletSpawnNetEvents)
        {
            if (bulletSpawnNetEvents.IsNullOrEmpty())
            {
                return;
            }

            foreach (var bulletSpawnNetEvent in bulletSpawnNetEvents)
            {
                _matchDataService.AddBullet(bulletSpawnNetEvent.BulletId, bulletSpawnNetEvent.BelongToPlayerId,
                    bulletSpawnNetEvent.Position);
                _matchNetEventsDataService.BulletSpawnNetEvents.Add(bulletSpawnNetEvent);
            }
        }
    }
}