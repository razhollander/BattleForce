using System.Collections.Generic;
using System.Linq;
using Core.Game.Domains.GamePlay.Presentation.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Shared.ServerToClientModels;
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
        
        public SimulationStatePacketsHandler(IClientNetworkManager networkManager, IMatchDataService matchDataService)
        {
            _networkManager = networkManager;
            _matchDataService = matchDataService;
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
            if (latestTickReceivedFromServer <= LatestTickProcessedFromServer)
            {
                LogService.LogTopic("Didn't receive any state since last tick", LogTopicType.ClientNetwork);
                return;
            }

            var simulationState = _statesPerTIck[latestTickReceivedFromServer];
            foreach (var player in _matchDataService.Players)
            {
                var playerState = simulationState.GetPlayer(player.PlayerId);
                player.Spaceship.Transform.Position = playerState.Spaceship.Transform.Position;
                player.Spaceship.Transform.RotationVector = playerState.Spaceship.Transform.RotationVector;
            }

            LatestTickProcessedFromServer = latestTickReceivedFromServer;
            _statesPerTIck.Clear();
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