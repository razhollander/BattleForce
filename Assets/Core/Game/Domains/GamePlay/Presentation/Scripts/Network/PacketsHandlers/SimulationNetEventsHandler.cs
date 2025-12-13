using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Presentation.Features.Player.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Presentation.Scripts.Presentation;
using Core.Game.Domains.GamePlay.Shared;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Shared.S2CModels.PacketEvents;
using Core.Scripts.Network;
using CoreDomain.Scripts.Extensions;
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

        public SimulationNetEventsHandler(IMatchDataService matchDataService, IMatchNetEventsDataService matchNetEventsDataService, IClientNetworkManager networkManager, IPlayerControllers playerControllers, NetworkConfig networkConfig, IClientPresentationTickProcessor clientPresentationTickProcessor)
        {
            _matchDataService = matchDataService;
            _matchNetEventsDataService = matchNetEventsDataService;
            _networkManager = networkManager;
            _playerControllers = playerControllers;
            _networkConfig = networkConfig;
            _clientPresentationTickProcessor = clientPresentationTickProcessor;
        }

        public void ProcessPlayerJoinedEvents(List<PlayerJoinAcceptPacketS2C> playerJoinAcceptNetEvents, ref int clientTick)
        {
            foreach (var playerJoinAcceptNetEvent in playerJoinAcceptNetEvents)
            {
                var playerId = playerJoinAcceptNetEvent.PlayerId;
                var isLocalPlayer = playerJoinAcceptNetEvent.NetPeerId == _networkManager.LocalPeerId;
                LogService.LogTopic($"Join packet accepted processed, isLocalPlayer:{isLocalPlayer}, player id: " + playerId, LogTopicType.ClientNetwork);
                var playerModel = _matchDataService.AddPlayer(playerId, playerJoinAcceptNetEvent.PlayerName, playerJoinAcceptNetEvent.SpaceshipState);
                _playerControllers.CreatePlayer(playerModel.PlayerId);
                
                if (isLocalPlayer)
                {
                    SetupLocalPlayer(out clientTick, playerJoinAcceptNetEvent, playerModel);
                }
            }
        }
        
        private void SetupLocalPlayer(out int clientTick, PlayerJoinAcceptPacketS2C playerJoinAcceptNetEvent,
            MatchPlayerModel playerModel)
        {
            var ticksPassedSinceServerSendPacket = (_networkManager.Ping / 1000f) / _networkConfig.DeltaTime;
            var tickWouldBeOnServerWhenReceiveMyPackets = (int)(ticksPassedSinceServerSendPacket * 2) + playerJoinAcceptNetEvent.OccuredOnTick;
            clientTick = tickWouldBeOnServerWhenReceiveMyPackets;
            _matchDataService.SetLocalPlayer(playerModel);
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