using System.Collections.Generic;
using System.Linq;
using Core.Game.Domains.GamePlay.Presentation.Features.Player.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Features.Player.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Presentation.Scripts.Presentation;
using Core.Game.Domains.GamePlay.Shared;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Shared.S2CModels.PacketEvents;
using Core.Game.Domains.GamePlay.Shared.S2CModels.PacketEvents.NetEvents;
using Core.Scripts.Network;
using CoreDomain.Scripts.Extensions;
using CoreDomain.Scripts.Services.CommandFactory;
using CoreDomain.Scripts.Services.Logger.Base;
using LiteNetLib;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Scripts.Network.PacketsHandlers
{
    public class FullTickPacketsHandler : IFullTickPacketsHandler
    {
        private readonly IClientNetworkManager _networkManager;
        private readonly IMatchDataService _matchDataService;
        private readonly Dictionary<int, FullTickPacket> _fullTickPackets = new();
        private readonly SimulationNetEventsHandler _simulationNetEventsHandler;
        public int LastProcessedTickFromServer { get; private set; }

        public FullTickPacketsHandler(NetworkConfig networkConfig, IClientNetworkManager networkManager,
            IMatchDataService matchDataService, IMatchNetEventsDataService matchNetEventsDataService,
            IPlayerControllers playerControllers, IClientPresentationTickProcessor clientPresentationTickProcessor, ICommandFactory commandFactory)
        {
            _networkManager = networkManager;
            _matchDataService = matchDataService;
            _simulationNetEventsHandler = new SimulationNetEventsHandler(matchDataService, matchNetEventsDataService,
                networkManager, playerControllers, networkConfig, clientPresentationTickProcessor, commandFactory);
        }

        public void RegisterListeners()
        {
            _networkManager.SubscribeNetSerializable<FullTickPacket, NetPeer>(OnFullTickReceived);
        }

        public int ProcessStateLatestTick(int clientTick)
        {
            clientTick++;
            if (_fullTickPackets.IsNullOrEmpty())
            {
                return clientTick;
            }

            var latestTickReceivedFromServer = _fullTickPackets.Keys.Max();
            var latestFullTickPacket = _fullTickPackets[latestTickReceivedFromServer];

            if (latestTickReceivedFromServer <= LastProcessedTickFromServer)
            {
#if Logs
                LogService.LogTopic("Didn't receive any state since last tick", LogTopicType.ClientNetwork);
#endif
                return clientTick;
            }

            ProcessPlayerJoinedEvents(latestFullTickPacket.PlayerJoinAcceptNetEvents, ref clientTick);
            ProcessBulletSpawnedEvents(latestFullTickPacket.BulletSpawnNetEvents);
            ProcessPlayerTakeDamageEvents(latestFullTickPacket.PlayerTakeDamageNetEvents);
            ProcessBulletDestroyedEvents(latestFullTickPacket.BulletDestroyedNetEvents);
            var simulationState = latestFullTickPacket.CurrentSimulationState;
            UpdatePlayersDeltas(simulationState);
            UpdateBulletsTransform(simulationState);

            LastProcessedTickFromServer = latestTickReceivedFromServer;
            _fullTickPackets.Clear();
            return clientTick;
        }

        private void ProcessBulletDestroyedEvents(List<BulletDestroyedNetEventS2C> bulletDestroyedNetEvents)
        {
            if (bulletDestroyedNetEvents.IsNullOrEmpty())
            {
                return;
            }

            var unProcessedBulletDestroyedEvents =
                bulletDestroyedNetEvents.FindAll(x => x.OccuredOnTick > LastProcessedTickFromServer);
            _simulationNetEventsHandler.ProcessBulletDestroyedEvents(unProcessedBulletDestroyedEvents);
        }

        private void ProcessPlayerTakeDamageEvents(List<PlayerTakeDamageNetEventS2C> playerTakeDamageNetEvents)
        {
            
            if (playerTakeDamageNetEvents.IsNullOrEmpty())
            {
                return;
            }

            var unProcessedPlayerDamageEvents =
                playerTakeDamageNetEvents.FindAll(x => x.OccuredOnTick > LastProcessedTickFromServer);
            _simulationNetEventsHandler.ProcessPlayerTakeDamageEvents(unProcessedPlayerDamageEvents);
        }

        private void ProcessPlayerJoinedEvents(List<PlayerJoinAcceptPacketS2C> playerJoinAcceptNetEvents, ref int clientTick)
        {
            if (playerJoinAcceptNetEvents.IsNullOrEmpty())
            {
                return;
            }

            var unProcessedPlayerJoinedEvents = 
                playerJoinAcceptNetEvents.FindAll(x => x.OccuredOnTick > LastProcessedTickFromServer);
            _simulationNetEventsHandler.ProcessPlayerJoinedEvents(unProcessedPlayerJoinedEvents, ref clientTick);
        }

        private void ProcessBulletSpawnedEvents(List<BulletSpawnNetEventS2C> bulletSpawnNetEvents)
        {
            if (bulletSpawnNetEvents.IsNullOrEmpty())
            {
                return;
            }

            var unProcessedBulletSpawnedEvents =
                bulletSpawnNetEvents.FindAll(x => x.OccuredOnTick > LastProcessedTickFromServer);
            _simulationNetEventsHandler.ProcessBulletSpawnEvents(unProcessedBulletSpawnedEvents);
        }

        private void UpdatePlayersDeltas(SimulationStateS2C simulationState)
        {
            foreach (var player in _matchDataService.Players)
            {
                var playerState = simulationState.GetPlayer(player.PlayerId);
                player.Spaceship.Transform.Position = playerState.Spaceship.Transform.Position;
                player.Spaceship.Transform.Direction = playerState.Spaceship.Transform.Direction;
                player.Spaceship.Shoot.CooldownSecondsLeft = playerState.Spaceship.Shoot.CooldownSecondsLeft;
            }
        }

        private void UpdateBulletsTransform(SimulationStateS2C simulationState)
        {
            foreach (var bullet in _matchDataService.Bullets)
            {
                var bulletState = simulationState.GetBullet(bullet.Id);
                bullet.Position = bulletState.Position;
            }
        }
        
        private void OnFullTickReceived(FullTickPacket fullTickPacket, NetPeer _)
        {
#if Logs
            LogService.LogTopic("FullTickPacket accepted received", LogTopicType.ClientNetwork);
#endif
            var tick = fullTickPacket.Tick;
            _fullTickPackets.Add(tick, fullTickPacket);
        }

        public void InitExitPoint()
        {
            UnregisterListeners();
        }

        private void UnregisterListeners()
        {
            _networkManager.RemoveSubscription<FullTickPacket>();
        }
    }
}