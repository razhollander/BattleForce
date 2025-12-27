using System.Linq;
using Core.Game.Domains.GamePlay.Presentation.Features.Player.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Presentation.Scripts.Presentation;
using Core.Game.Domains.GamePlay.Shared.C2SModels;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Shared.S2CModels.PacketEvents;
using Core.Game.Domains.GamePlay.Shared.S2CModels.PacketEvents.NetEvents;
using Core.Scripts.Extensions;
using Core.Scripts.Network;
using Core.Scripts.Utils;
using CoreDomain.Scripts.Services.CommandFactory;
using CoreDomain.Scripts.Services.Logger.Base;
using LiteNetLib;

namespace Core.Game.Domains.GamePlay.Presentation.Scripts.Network.PacketsHandlers
{
    public class FullTickPacketsHandler : IFullTickPacketsHandler
    {
        private readonly IClientNetworkManager _networkManager;
        private readonly IMatchDataService _matchDataService;
        private readonly SimulationNetEventsHandler _simulationNetEventsHandler;
        private readonly CapacityDict<int, FullTickPacket> _fullTickPackets;
        private readonly CapacityList<PlayerJoinAcceptPacketS2C> _cachedUnprocessedPlayerJoinedEvents;
        private readonly CapacityList<BulletSpawnNetEventS2C> _cachedUnprocessedBulletSpawnedEvents;
        private readonly CapacityList<PlayerTakeDamageNetEventS2C> _cachedUnprocessedPlayerTakeDamageEvents;
        private readonly CapacityList<BulletDestroyedNetEventS2C> _cachedUnprocessedBulletDestroyedEvents;
        private readonly ConcurrentPool<FullTickPacket> _fullTickPacketsPool;
        public PacketTypeS2C PacketType => PacketTypeS2C.FullTick;
        public int LastProcessedTickFromServer { get; private set; }

        public FullTickPacketsHandler(NetworkConfig networkConfig, IClientNetworkManager networkManager,
            IMatchDataService matchDataService, IMatchNetEventsDataService matchNetEventsDataService,
            IPlayerControllers playerControllers, IClientPresentationTickProcessor clientPresentationTickProcessor, ICommandFactory commandFactory)
        {
            _networkManager = networkManager;
            _matchDataService = matchDataService;

            _simulationNetEventsHandler = new SimulationNetEventsHandler(matchDataService, matchNetEventsDataService,
                networkManager, playerControllers, networkConfig, clientPresentationTickProcessor, commandFactory);
            _fullTickPackets = new CapacityDict<int, FullTickPacket>(networkConfig.MaxCap.FullTickPacketsNetEvents);
            _cachedUnprocessedPlayerJoinedEvents = new CapacityList<PlayerJoinAcceptPacketS2C>(networkConfig.MaxCap.PlayerJoinAcceptNetEvents);
            _cachedUnprocessedBulletSpawnedEvents = new CapacityList<BulletSpawnNetEventS2C>(networkConfig.MaxCap.BulletSpawnNetEvents);
            _cachedUnprocessedPlayerTakeDamageEvents = new CapacityList<PlayerTakeDamageNetEventS2C>(networkConfig.MaxCap.PlayerTakeDamageNetEvents);
            _cachedUnprocessedBulletDestroyedEvents = new CapacityList<BulletDestroyedNetEventS2C>(networkConfig.MaxCap.BulletDestroyedNetEvents);
            _fullTickPacketsPool = new ConcurrentPool<FullTickPacket>(() => new FullTickPacket(networkConfig.MaxCap), networkConfig.MaxCap.FullTickPacketsNetEvents);
        }

        public void RegisterListeners()
        {
            _networkManager.RegisterPacketsObserver(this);
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
                LogService.LogTopic("Didn't receive any state since last tick", LogTopicType.ClientNetwork);
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

            foreach (var kvp in _fullTickPackets)
            {
                _fullTickPacketsPool.Return(kvp.Value);
            }

            _fullTickPackets.Clear();

            return clientTick;
        }

        private void ProcessBulletDestroyedEvents(FixedUnorderedList<BulletDestroyedNetEventS2C> bulletDestroyedNetEvents)
        {
            _cachedUnprocessedBulletDestroyedEvents.Clear();

            foreach (var netEvent in bulletDestroyedNetEvents.AsSpan())
            {
                if (netEvent.OccuredOnTick > LastProcessedTickFromServer)
                {
                    _cachedUnprocessedBulletDestroyedEvents.Add(netEvent);
                }
            }

            if (!_cachedUnprocessedBulletDestroyedEvents.IsNullOrEmpty())
            {
                _simulationNetEventsHandler.ProcessBulletDestroyedEvents(_cachedUnprocessedBulletDestroyedEvents);
            }
        }

        private void ProcessPlayerTakeDamageEvents(FixedUnorderedList<PlayerTakeDamageNetEventS2C> playerTakeDamageNetEvents)
        {
            _cachedUnprocessedPlayerTakeDamageEvents.Clear();

            foreach (var netEvent in playerTakeDamageNetEvents.AsSpan())
            {
                if (netEvent.OccuredOnTick > LastProcessedTickFromServer)
                {
                    _cachedUnprocessedPlayerTakeDamageEvents.Add(netEvent);
                }
            }

            if (!_cachedUnprocessedPlayerTakeDamageEvents.IsNullOrEmpty())
            {
                _simulationNetEventsHandler.ProcessPlayerTakeDamageEvents(_cachedUnprocessedPlayerTakeDamageEvents);
            }
        }


        private void ProcessPlayerJoinedEvents(FixedUnorderedList<PlayerJoinAcceptPacketS2C> playerJoinAcceptNetEvents, ref int clientTick)
        {
            _cachedUnprocessedPlayerJoinedEvents.Clear();

            foreach (var netEvent in playerJoinAcceptNetEvents.AsSpan())
            {
                if (netEvent.OccuredOnTick > LastProcessedTickFromServer)
                {
                    _cachedUnprocessedPlayerJoinedEvents.Add(netEvent);
                }
            }

            if (!_cachedUnprocessedPlayerJoinedEvents.IsNullOrEmpty())
            {
                _simulationNetEventsHandler.ProcessPlayerJoinedEvents(_cachedUnprocessedPlayerJoinedEvents, ref clientTick);
            }
        }

        private void ProcessBulletSpawnedEvents(FixedUnorderedList<BulletSpawnNetEventS2C> bulletSpawnNetEvents)
        {
            _cachedUnprocessedBulletSpawnedEvents.Clear();

            foreach (var netEvent in bulletSpawnNetEvents.AsSpan())
            {
                if (netEvent.OccuredOnTick > LastProcessedTickFromServer)
                {
                    _cachedUnprocessedBulletSpawnedEvents.Add(netEvent);
                }
            }

            if (!_cachedUnprocessedBulletSpawnedEvents.IsNullOrEmpty())
            {
                _simulationNetEventsHandler.ProcessBulletSpawnEvents(_cachedUnprocessedBulletSpawnedEvents);
            }
        }

        private void UpdatePlayersDeltas(SimulationStateS2C simulationState)
        {
            foreach (var player in _matchDataService.Players)
            {
                var playerState = simulationState.GetPlayerById(player.PlayerId);
                player.Spaceship.Transform.Position = playerState.Spaceship.Transform.Position;
                player.Spaceship.Transform.Direction = playerState.Spaceship.Transform.Direction;
                player.Spaceship.Shoot.CooldownSecondsLeft = playerState.Spaceship.Shoot.CooldownSecondsLeft;
            }
        }

        private void UpdateBulletsTransform(SimulationStateS2C simulationState)
        {
            foreach (var bullet in _matchDataService.Bullets)
            {
                var bulletState = simulationState.GetBulletById(bullet.Id);
                bullet.Position = bulletState.Position;
            }
        }
        
        public void OnPacketReceived(NetPacketReader reader)
        {
            var newPacket = _fullTickPacketsPool.Get();
            newPacket.Deserialize(reader);
            OnFullTickReceived(newPacket);
        }
        
        private void OnFullTickReceived(FullTickPacket fullTickPacket)
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
            _networkManager.UnregisterPacketsObserver(this);
        }
    }
}