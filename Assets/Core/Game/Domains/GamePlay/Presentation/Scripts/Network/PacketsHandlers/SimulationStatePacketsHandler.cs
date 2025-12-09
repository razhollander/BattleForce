using System.Collections.Generic;
using System.Linq;
using Core.Game.Domains.GamePlay.Presentation.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using CoreDomain.Scripts.Extensions;
using CoreDomain.Scripts.Services.Logger.Base;
using LiteNetLib;

namespace Core.Game.Domains.GamePlay.Presentation.Scripts.Network.PacketsHandlers
{
    public class SimulationStatePacketsHandler : ISimulationStatePacketsHandler
    {
        private readonly IClientNetworkManager _networkManager;
        private readonly IMatchDataService _matchDataService;
        private readonly Dictionary<int, SimulationStateS2C> _statesPerTIck = new();
        public int LatestTickProcessedFromServer { get; private set; }
        private SimulationNetEventsHandler _simulationNetEventsHandler;
        
        public SimulationStatePacketsHandler(IClientNetworkManager networkManager, IMatchDataService matchDataService, IMatchNetEventsDataService matchNetEventsDataService)
        {
            _networkManager = networkManager;
            _matchDataService = matchDataService;
            _simulationNetEventsHandler = new SimulationNetEventsHandler(matchDataService, matchNetEventsDataService);
        }

        public void RegisterListeners()
        {
            _networkManager.SubscribeNetSerializable<SimulationStateS2C, NetPeer>(OnSimulationStateReceived);
        }

        public void ProcessStateLatestTick()
        {
            if (_statesPerTIck.IsNullOrEmpty())
            {
                return;
            }
            
            var latestTickReceivedFromServer = _statesPerTIck.Keys.Max();
            var simulationState = _statesPerTIck[latestTickReceivedFromServer];

            if (latestTickReceivedFromServer <= LatestTickProcessedFromServer)
            {
                LogService.LogTopic("Didn't receive any state since last tick", LogTopicType.ClientNetwork);
                return;
            }

            _simulationNetEventsHandler.ProcessBulletSpawnEvents(simulationState.Bullets, simulationState.BulletSpawnNetEvents);
            UpdatePlayersTransform(simulationState);
            UpdateBulletsTransform(simulationState);

            LatestTickProcessedFromServer = latestTickReceivedFromServer;
            _statesPerTIck.Clear();
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
        
        private void OnSimulationStateReceived(SimulationStateS2C simulationState, NetPeer _)
        {
            LogService.LogTopic("Simulation state packet accepted received", LogTopicType.ClientNetwork);
            var tick = simulationState.Tick;
            _statesPerTIck.Add(tick, simulationState);
        }

        public void InitExitPoint()
        {
            UnregisterListeners();
        }

        private void UnregisterListeners()
        {
            _networkManager.RemoveSubscription<SimulationStateS2C>();
        }
    }
}