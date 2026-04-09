using Core.Game.Domains.GamePlay.Presentation.MatchMaking.Features.StartMatchButton.Scripts.Mvcs;
using Core.Game.Domains.GamePlay.Presentation.MatchMaking.Scripts.DataService;
using Core.Game.Domains.GamePlay.Presentation.MatchMaking.Scripts.TickProcessor;
using Core.Game.Domains.GamePlay.Presentation.Scripts.Network;
using Core.Game.Domains.GamePlay.Presentation.Scripts.Network.PacketsHandlers;
using Core.Game.Domains.GamePlay.Presentation.Scripts.PresentationEvents;
using Core.Game.Domains.GamePlay.Presentation.Scripts.TickProcessors;
using Core.Game.Domains.GamePlay.Shared.C2SModels;
using Core.Game.Domains.GamePlay.Shared.S2CModels.PacketEvents;
using Core.Game.Domains.GamePlay.Shared.S2CModels.PacketEvents.NetEvents;
using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels.MatchMaking;
using Core.Game.Domains.GamePlay.Shared.S2CModels.MatchMaking.PacketEvents.NetEvents;
using Core.Scripts.Extensions;
using Core.Scripts.Extensions.Linq;
using Core.Scripts.Network;
using Core.Scripts.Utils;
using Core.Scripts.Utils.CustomCollections;
using CoreDomain.Scripts.Services.CommandFactory;
using CoreDomain.Scripts.Services.Logger.Base;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Presentation.MatchMaking.Scripts.Network.PacketsHandlers
{
    public class MatchMakingFullTickPacketsHandler : IFullTickPacketsHandler
    {
        private readonly IClientNetworkManager _networkManager;
        private readonly IMatchMakingDataService _matchDataService;
        private readonly PresentationMatchMakingNetEventsHandler _presentationNetEventsHandler;
        private readonly CapacityDict<int, MatchMakingFullTickPacketS2C> _fullTickPackets;
        private readonly CapacityList<MatchMakingPlayerJoinAcceptPacketS2C> _cachedUnprocessedPlayerJoinedEvents;
        private readonly CapacityList<BulletSpawnNetEventS2C> _cachedUnprocessedBulletSpawnedEvents;
        private readonly CapacityList<BulletDestroyedNetEventS2C> _cachedUnprocessedBulletDestroyedEvents;
        private readonly CapacityList<PlayerSwitchTeamNetEventS2C> _cachedUnprocessedPlayerSwitchTeamEvents;
        private readonly CapacityList<StartMatchCountdownNetEventS2C> _cachedUnprocessedStartMatchCountdownEvents;
        private readonly CapacityList<StopMatchCountdownNetEventS2C> _cachedUnprocessedStopMatchCountdownEvents;
        private readonly CapacityList<StartMatchEligibleChangedNetEventS2C> _cachedUnprocessedStartMatchEligibleChangedEvents;
        private readonly ConcurrentPool<MatchMakingFullTickPacketS2C> _fullTickPacketsPool;
        public PacketTypeS2C PacketType => PacketTypeS2C.MatchMakingFullTick;
        public int LastProcessedTickFromServer { get; private set; }

        public MatchMakingFullTickPacketsHandler(NetworkConfig networkConfig, IClientNetworkManager networkManager,
            IMatchMakingDataService matchDataService, ICachedPresentationEventsService cachedPresentationEventsService, ICommandFactory commandFactory,
            IStartMatchButtonController startMatchButtonController)
        {
            _networkManager = networkManager;
            _matchDataService = matchDataService;

            _presentationNetEventsHandler = new PresentationMatchMakingNetEventsHandler(matchDataService, cachedPresentationEventsService, commandFactory, startMatchButtonController);

            _fullTickPackets = new CapacityDict<int, MatchMakingFullTickPacketS2C>(networkConfig.MaxCap.FullTickPacketsNetEvents);
            _cachedUnprocessedPlayerJoinedEvents = new CapacityList<MatchMakingPlayerJoinAcceptPacketS2C>(networkConfig.MaxCap.PlayerJoinAcceptNetEvents);
            _cachedUnprocessedBulletSpawnedEvents = new CapacityList<BulletSpawnNetEventS2C>(networkConfig.MaxCap.BulletSpawnNetEvents);
            _cachedUnprocessedBulletDestroyedEvents = new CapacityList<BulletDestroyedNetEventS2C>(networkConfig.MaxCap.BulletDestroyedNetEvents);
            _cachedUnprocessedStartMatchCountdownEvents = new CapacityList<StartMatchCountdownNetEventS2C>(networkConfig.MaxCap.StartMatchCountdownNetEvents);
            _cachedUnprocessedStopMatchCountdownEvents = new CapacityList<StopMatchCountdownNetEventS2C>(networkConfig.MaxCap.StopMatchCountdownNetEvents);
            _cachedUnprocessedPlayerSwitchTeamEvents = new CapacityList<PlayerSwitchTeamNetEventS2C>(networkConfig.MaxCap.PlayerSwitchTeamNetEvents);
            _cachedUnprocessedStartMatchEligibleChangedEvents = new CapacityList<StartMatchEligibleChangedNetEventS2C>(networkConfig.MaxCap.StartMatchEligibleChangedNetEvents);

            _fullTickPacketsPool =
                new ConcurrentPool<MatchMakingFullTickPacketS2C>(() => new MatchMakingFullTickPacketS2C(networkConfig.MaxCap), networkConfig.MaxCap.FullTickPacketsNetEvents);
        }

        public void InitEntryPoint()
        {
            _networkManager.RegisterPacketsObserver(this);
        }

        public void ProcessStateLatestTick()
        {
            if (_fullTickPackets.IsNullOrEmpty())
            {
                return;
            }

            var latestTickReceivedFromServer = _fullTickPackets.Keys.Max();
            var latestFullTickPacket = _fullTickPackets[latestTickReceivedFromServer];

            if (latestTickReceivedFromServer <= LastProcessedTickFromServer)
            {
                LogService.LogTopic("Didn't receive any state since last tick", LogTopicType.ClientNetwork);

                return;
            }

            ProcessPlayerJoinedEvents(latestFullTickPacket.PlayerJoinAcceptNetEvents);
            ProcessBulletSpawnedEvents(latestFullTickPacket.BulletSpawnNetEvents);
            ProcessBulletDestroyedEvents(latestFullTickPacket.BulletDestroyedNetEvents);
            ProcessPlayerSwitchTeamEvents(latestFullTickPacket.PlayerSwitchTeamNetEvents);
            ProcessStartMatchCountdownEvents(latestFullTickPacket.StartMatchCountdownNetEvents);
            ProcessStopMatchCountdownEvents(latestFullTickPacket.StopMatchCountdownNetEvents);
            ProcessStartMatchEligibleChangedEvents(latestFullTickPacket.StartMatchEligibleChangedNetEvents);
            var simulationState = latestFullTickPacket.CurrentSimulationState;
            UpdatePlayersDeltas(simulationState);
            UpdateBulletsTransform(simulationState);

            LastProcessedTickFromServer = latestTickReceivedFromServer;

            foreach (var kvp in _fullTickPackets)
            {
                _fullTickPacketsPool.Return(kvp.Value);
            }

            _fullTickPackets.Clear();
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
                _presentationNetEventsHandler.ProcessBulletDestroyedEvents(_cachedUnprocessedBulletDestroyedEvents);
            }
        }


        private void ProcessPlayerJoinedEvents(FixedClassUnorderedList<MatchMakingPlayerJoinAcceptPacketS2C> playerJoinAcceptNetEvents)
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
                _presentationNetEventsHandler.ProcessPlayerJoinedEvents(_cachedUnprocessedPlayerJoinedEvents);
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
                _presentationNetEventsHandler.ProcessBulletSpawnEvents(_cachedUnprocessedBulletSpawnedEvents);
            }
        }

        private void ProcessPlayerSwitchTeamEvents(FixedUnorderedList<PlayerSwitchTeamNetEventS2C> playerSwitchTeamNetEvents)
        {
            _cachedUnprocessedPlayerSwitchTeamEvents.Clear();

            foreach (var netEvent in playerSwitchTeamNetEvents.AsSpan())
            {
                if (netEvent.OccuredOnTick > LastProcessedTickFromServer)
                {
                    _cachedUnprocessedPlayerSwitchTeamEvents.Add(netEvent);
                }
            }

            if (!_cachedUnprocessedPlayerSwitchTeamEvents.IsNullOrEmpty())
            {
                _presentationNetEventsHandler.ProcessPlayerSwitchTeamEvents(_cachedUnprocessedPlayerSwitchTeamEvents);
            }
        }

        private void ProcessStartMatchCountdownEvents(FixedUnorderedList<StartMatchCountdownNetEventS2C> startMatchCountdownNetEvents)
        {
            _cachedUnprocessedStartMatchCountdownEvents.Clear();

            foreach (var netEvent in startMatchCountdownNetEvents.AsSpan())
            {
                if (netEvent.OccuredOnTick > LastProcessedTickFromServer)
                {
                    _cachedUnprocessedStartMatchCountdownEvents.Add(netEvent);
                }
            }

            if (!_cachedUnprocessedStartMatchCountdownEvents.IsNullOrEmpty())
            {
                _presentationNetEventsHandler.ProcessStartMatchCountdownEvents(_cachedUnprocessedStartMatchCountdownEvents);
            }
        }

        private void ProcessStopMatchCountdownEvents(FixedUnorderedList<StopMatchCountdownNetEventS2C> stopMatchCountdownNetEvents)
        {
            _cachedUnprocessedStopMatchCountdownEvents.Clear();

            foreach (var netEvent in stopMatchCountdownNetEvents.AsSpan())
            {
                if (netEvent.OccuredOnTick > LastProcessedTickFromServer)
                {
                    _cachedUnprocessedStopMatchCountdownEvents.Add(netEvent);
                }
            }

            if (!_cachedUnprocessedStopMatchCountdownEvents.IsNullOrEmpty())
            {
                _presentationNetEventsHandler.ProcessStopMatchCountdownEvents(_cachedUnprocessedStopMatchCountdownEvents);
            }
        }

        private void ProcessStartMatchEligibleChangedEvents(FixedUnorderedList<StartMatchEligibleChangedNetEventS2C> events)
        {
            _cachedUnprocessedStartMatchEligibleChangedEvents.Clear();

            foreach (var netEvent in events.AsSpan())
            {
                if (netEvent.OccuredOnTick > LastProcessedTickFromServer)
                {
                    _cachedUnprocessedStartMatchEligibleChangedEvents.Add(netEvent);
                }
            }

            if (!_cachedUnprocessedStartMatchEligibleChangedEvents.IsNullOrEmpty())
            {
                _presentationNetEventsHandler.ProcessStartMatchEligibleChangedEvents(_cachedUnprocessedStartMatchEligibleChangedEvents);
            }
        }

        private void UpdatePlayersDeltas(MatchMakingSimulationStateS2C simulationState)
        {
            foreach (var player in _matchDataService.Players)
            {
                var playerState = simulationState.GetPlayerById(player.PlayerId);
                player.Spaceship.Transform.Position = playerState.Spaceship.Transform.Position;
                player.Spaceship.Transform.Direction = playerState.Spaceship.Transform.Direction;
                player.Spaceship.Transform.Velocity = playerState.Spaceship.Transform.Velocity;
                player.Spaceship.Transform.AngularVelocity = playerState.Spaceship.Transform.AngularVelocity;
                player.Spaceship.Shoot.CooldownSecondsLeft = playerState.Spaceship.Shoot.CooldownSecondsLeft;
            }
        }

        private void UpdateBulletsTransform(MatchMakingSimulationStateS2C simulationState)
        {
            foreach (var bullet in _matchDataService.Bullets)
            {
                var bulletState = simulationState.GetBulletById(bullet.Id);
                bullet.Position = bulletState.Position;
            }
        }

        public void OnPacketReceived(NetDataReader reader)
        {
            var newPacket = _fullTickPacketsPool.Get();
            newPacket.Deserialize(reader);
            OnFullTickReceived(newPacket);
        }

        private void OnFullTickReceived(MatchMakingFullTickPacketS2C fullTickPacket)
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
