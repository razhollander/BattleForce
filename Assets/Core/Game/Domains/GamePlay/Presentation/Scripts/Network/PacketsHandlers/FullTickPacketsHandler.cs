using System.Collections.Generic;
using System.Linq;
using Core.Game.Domains.GamePlay.Presentation.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using CoreDomain.Scripts.Extensions;
using CoreDomain.Scripts.Services.Logger.Base;
using LiteNetLib;

namespace Core.Game.Domains.GamePlay.Presentation.Scripts.Network.PacketsHandlers
{
    public class FullTickPacketsHandler : IFullTickPacketsHandler
    {
        private readonly IClientNetworkManager _networkManager;
        private readonly IMatchDataService _matchDataService;
        private readonly Dictionary<int, FullTickPacket> _fullTickPackets = new();
        private readonly SimulationNetEventsHandler _simulationNetEventsHandler;
        public int LatestTickProcessedFromServer { get; private set; }
        
        public FullTickPacketsHandler(IClientNetworkManager networkManager, IMatchDataService matchDataService, IMatchNetEventsDataService matchNetEventsDataService)
        {
            _networkManager = networkManager;
            _matchDataService = matchDataService;
            _simulationNetEventsHandler = new SimulationNetEventsHandler(matchDataService, matchNetEventsDataService);
        }

        public void RegisterListeners()
        {
            _networkManager.SubscribeNetSerializable<FullTickPacket, NetPeer>(OnFullTickReceived);
        }

        public void ProcessStateLatestTick()
        {
            if (_fullTickPackets.IsNullOrEmpty())
            {
                return;
            }
            
            var latestTickReceivedFromServer = _fullTickPackets.Keys.Max();
            var latestFullTickPacket = _fullTickPackets[latestTickReceivedFromServer];

            if (latestTickReceivedFromServer <= LatestTickProcessedFromServer)
            {
                LogService.LogTopic("Didn't receive any state since last tick", LogTopicType.ClientNetwork);
                return;
            }

            var simulationState = latestFullTickPacket.CurrentSimulationState;
            _simulationNetEventsHandler.ProcessBulletSpawnEvents(latestFullTickPacket.BulletSpawnNetEvents);
            UpdatePlayersTransform(simulationState);
            UpdateBulletsTransform(simulationState);

            LatestTickProcessedFromServer = latestTickReceivedFromServer;
            _fullTickPackets.Clear();
        }

        private void UpdatePlayersTransform(SimulationStateS2C simulationState)
        {
            foreach (var player in _matchDataService.Players)
            {
                var playerState = simulationState.GetPlayer(player.PlayerId);
                player.Spaceship.Transform.Position = playerState.Spaceship.Transform.Position;
                player.Spaceship.Transform.Direction = playerState.Spaceship.Transform.Direction;
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
            LogService.LogTopic("FullTickPacket accepted received", LogTopicType.ClientNetwork);
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