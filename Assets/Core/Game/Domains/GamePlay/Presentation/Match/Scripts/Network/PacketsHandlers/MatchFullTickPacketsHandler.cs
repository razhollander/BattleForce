using System.Linq;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.DataService;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.TickProcessor;
using Core.Game.Domains.GamePlay.Presentation.Scripts.Network;
using Core.Game.Domains.GamePlay.Presentation.Scripts.Network.PacketsHandlers;
using Core.Game.Domains.GamePlay.Presentation.Scripts.PresentationEvents;
using Core.Game.Domains.GamePlay.Presentation.Scripts.TickProcessors;
using Core.Game.Domains.GamePlay.Shared.C2SModels;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Shared.S2CModels.PacketEvents;
using Core.Game.Domains.GamePlay.Shared.S2CModels.PacketEvents.NetEvents;
using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels;
using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels.PacketEvents.NetEvents;
using Core.Game.Domains.GamePlay.Shared.Scripts.Utils;
using Core.Scripts.Extensions;
using Core.Scripts.Network;
using Core.Scripts.Utils;
using Core.Scripts.Utils.CustomCollections;
using CoreDomain.Scripts.Services.CommandFactory;
using CoreDomain.Scripts.Services.Logger.Base;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Network.PacketsHandlers
{
    public class MatchFullTickPacketsHandler : IFullTickPacketsHandler
    {
        private readonly NetworkConfig _networkConfig;
        private readonly IClientNetworkManager _networkManager;
        private readonly IMatchDataService _matchDataService;
        private readonly PresentationMatchNetEventsHandler _presentationNetEventsHandler;
        private readonly CapacityDict<int, MatchFullTickPacketS2C> _fullTickPackets;
        private readonly CapacityList<PlayerRejoinAcceptPacketS2C> _cachedUnprocessedPlayerRejoinedEvents;
        private readonly CapacityList<BulletSpawnNetEventS2C> _cachedUnprocessedBulletSpawnedEvents;
        private readonly CapacityList<PlayerTakeDamageNetEventS2C> _cachedUnprocessedPlayerTakeDamageEvents;
        private readonly CapacityList<PlayerDiedNetEventS2C> _cachedUnprocessedPlayerDiedEvents;
        private readonly CapacityList<BulletDestroyedNetEventS2C> _cachedUnprocessedBulletDestroyedEvents;
        private readonly CapacityList<PlayersSwapNetEventS2C> _cachedUnprocessedPlayerSwapEvents;
        private readonly CapacityList<TalentCardObtainedNetEventS2C> _cachedUnprocessedTalentCardObtainedEvents;
        private readonly CapacityList<TalentCardHitNetEventS2C> _cachedUnprocessedTalentCardHitEvents;
        private readonly CapacityList<PowerUpBallSpawnedNetEventS2C> _cachedUnprocessedPowerUpBallSpawnedEvents;
        private readonly CapacityList<PowerUpBallObtainedNetEventS2C> _cachedUnprocessedPowerUpBallObtainedEvents;
        private readonly CapacityList<StageEndNetEventS2C> _cachedUnprocessedStageEndEvents;
        private readonly CapacityList<TeamLostNetEventS2C> _cachedUnprocessedTeamLostEvents;
        private readonly CapacityList<TalentSwitchNetEventS2C> _cachedUnprocessedTalentSwitchEvents;
        private readonly CapacityList<GainBoltsNetEventS2C> _cachedUnprocessedGainBoltsEvents;
        private readonly CapacityList<PlayerToEnvironmentTeleportGateCollisionNetEventS2C> _cachedUnprocessedPlayerToEnvironmentTeleportCollisionEvents;
        private readonly CapacityList<EnvironmentSpringPlayerCollisionNetEventS2C> _cachedUnprocessedEnvironmentSpringPlayerCollisionEvents;
        private readonly CapacityList<PreparationPhaseEndedNetEventS2C> _cachedUnprocessedPreparationPhaseEndedEvents;
        private readonly ConcurrentPool<MatchFullTickPacketS2C> _fullTickPacketsPool;
        public PacketTypeS2C PacketType => PacketTypeS2C.MatchFullTick;
        public int LastProcessedTickFromServer { get; private set; }

        public MatchFullTickPacketsHandler(NetworkConfig networkConfig, SharedGamePlayConfig sharedGamePlayConfig, IClientNetworkManager networkManager,
            IMatchDataService matchDataService, ICachedPresentationEventsService iCachedPresentationEventsService, Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Timer.IMatchPlayerTimersService matchPlayerTimersService,
            IClientMatchPresentationTickProcessor clientPresentationTickProcessor, ICommandFactory commandFactory, ITickCounterService tickCounterService)
        {
            _networkConfig = networkConfig;
            _networkManager = networkManager;
            _matchDataService = matchDataService;

            _presentationNetEventsHandler = new PresentationMatchNetEventsHandler(matchDataService, iCachedPresentationEventsService, networkManager, networkConfig, clientPresentationTickProcessor, commandFactory, tickCounterService, matchPlayerTimersService);
            _fullTickPackets = new CapacityDict<int, MatchFullTickPacketS2C>(networkConfig.MaxCap.FullTickPacketsNetEvents);
            _cachedUnprocessedPlayerRejoinedEvents = new CapacityList<PlayerRejoinAcceptPacketS2C>(networkConfig.MaxCap.PlayerJoinAcceptNetEvents);
            _cachedUnprocessedBulletSpawnedEvents = new CapacityList<BulletSpawnNetEventS2C>(networkConfig.MaxCap.BulletSpawnNetEvents);
            _cachedUnprocessedPlayerTakeDamageEvents = new CapacityList<PlayerTakeDamageNetEventS2C>(networkConfig.MaxCap.PlayerTakeDamageNetEvents);
            _cachedUnprocessedPlayerDiedEvents = new CapacityList<PlayerDiedNetEventS2C>(networkConfig.MaxCap.PlayerDiedNetEvents);
            _cachedUnprocessedBulletDestroyedEvents = new CapacityList<BulletDestroyedNetEventS2C>(networkConfig.MaxCap.BulletDestroyedNetEvents);
            _cachedUnprocessedPlayerSwapEvents = new CapacityList<PlayersSwapNetEventS2C>(networkConfig.MaxCap.PlayerSwapNetEvents);
            _cachedUnprocessedTalentCardObtainedEvents = new CapacityList<TalentCardObtainedNetEventS2C>(networkConfig.MaxCap.TalentCardObtainedNetEvent);
            _cachedUnprocessedTalentCardHitEvents = new CapacityList<TalentCardHitNetEventS2C>(networkConfig.MaxCap.TalentCardHitNetEvents);
            _cachedUnprocessedPowerUpBallSpawnedEvents = new CapacityList<PowerUpBallSpawnedNetEventS2C>(networkConfig.MaxCap.PowerUpSpawnedNetEvents);
            _cachedUnprocessedPowerUpBallObtainedEvents = new CapacityList<PowerUpBallObtainedNetEventS2C>(networkConfig.MaxCap.PowerUpObtainedNetEvents);
            _cachedUnprocessedStageEndEvents = new CapacityList<StageEndNetEventS2C>(networkConfig.MaxCap.StageEndNetEvents);
            _cachedUnprocessedTeamLostEvents = new CapacityList<TeamLostNetEventS2C>(sharedGamePlayConfig.MaxTeamsAmount);
            _cachedUnprocessedTalentSwitchEvents = new CapacityList<TalentSwitchNetEventS2C>(networkConfig.MaxCap.TalentSwitchNetEvents);
            _cachedUnprocessedEnvironmentSpringPlayerCollisionEvents = new CapacityList<EnvironmentSpringPlayerCollisionNetEventS2C>(networkConfig.MaxCap.EnvironmentSpringPlayerCollisionNetEvents);
            _cachedUnprocessedGainBoltsEvents = new CapacityList<GainBoltsNetEventS2C>(networkConfig.MaxCap.GainBoltsNetEvents);
            _cachedUnprocessedPlayerToEnvironmentTeleportCollisionEvents = new CapacityList<PlayerToEnvironmentTeleportGateCollisionNetEventS2C>(networkConfig.MaxCap.PlayerToEnvironmentTeleportGateCollisionNetEvents);
            _cachedUnprocessedPreparationPhaseEndedEvents = new CapacityList<PreparationPhaseEndedNetEventS2C>(networkConfig.MaxCap.PreparationPhaseEndedNetEvents);
            _fullTickPacketsPool = new ConcurrentPool<MatchFullTickPacketS2C>(() => new MatchFullTickPacketS2C(networkConfig.MaxCap, sharedGamePlayConfig), networkConfig.MaxCap.FullTickPacketsNetEvents);
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
            
            ProcessPlayerRejoinedEvents(latestFullTickPacket.PlayerJoinAcceptNetEvents);
            ProcessBulletSpawnedEvents(latestFullTickPacket.BulletSpawnNetEvents);
            ProcessPlayerTakeDamageEvents(latestFullTickPacket.PlayerTakeDamageNetEvents);
            ProcessBulletDestroyedEvents(latestFullTickPacket.BulletDestroyedNetEvents);
            ProcessPlayerSwapEvents(latestFullTickPacket.PlayerSwapNetEvents);
            ProcessTalentCardHitEvents(latestFullTickPacket.TalentCardHitNetEvents);
            ProcessTalentCardObtainedEvents(latestFullTickPacket.TalentCardObtainedNetEvents);
            ProcessPowerUpBallSpawnedEvents(latestFullTickPacket.PowerUpSpawnedNetEvents);
            ProcessPowerUpBallObtainedEvents(latestFullTickPacket.PowerUpObtainedNetEvents);
            ProcessPlayerDiedEvents(latestFullTickPacket.PlayerDiedNetEvents);
            ProcessStageEndEvents(latestFullTickPacket.StageEndNetEvents);
            ProcessTeamLostEvents(latestFullTickPacket.TeamLostNetEvents);
            ProcessTalentSwitchEvents(latestFullTickPacket.TalentSwitchNetEvents);
            ProcessGainBoltsEvents(latestFullTickPacket.GainBoltsNetEvents);
            ProcessEnvironmentSpringPlayerCollisionEvents(latestFullTickPacket.EnvironmentSpringPlayerCollisionNetEvents);
            ProcessEnvironmentTeleportPlayerCollisionEvents(latestFullTickPacket.PlayerToEnvironmentTeleportGateCollisionNetEvents);
            ProcessPreparationPhaseEndedEvents(latestFullTickPacket.PreparationPhaseEndedNetEvents);
            var simulationState = latestFullTickPacket.CurrentSimulationState;
            UpdatePlayersDeltas(simulationState);
            UpdateBulletsTransform(simulationState);
            UpdatePowerUpBallsTransform(simulationState);
            UpdateRotatingWheels(latestTickReceivedFromServer);

            LastProcessedTickFromServer = latestTickReceivedFromServer;

            foreach (var kvp in _fullTickPackets)
            {
                _fullTickPacketsPool.Return(kvp.Value);
            }

            _fullTickPackets.Clear();
        }

        private void ProcessEnvironmentTeleportPlayerCollisionEvents(FixedUnorderedList<PlayerToEnvironmentTeleportGateCollisionNetEventS2C> playerToEnvironmentTeleportGateCollisionNetEvents)
        {
            _cachedUnprocessedPlayerToEnvironmentTeleportCollisionEvents.Clear();

            foreach (var netEvent in playerToEnvironmentTeleportGateCollisionNetEvents.AsSpan())
            {
                if (netEvent.OccuredOnTick > LastProcessedTickFromServer)
                {
                    _cachedUnprocessedPlayerToEnvironmentTeleportCollisionEvents.Add(netEvent);
                }
            }

            if (!_cachedUnprocessedPlayerToEnvironmentTeleportCollisionEvents.IsNullOrEmpty())
            {
                _cachedUnprocessedPlayerToEnvironmentTeleportCollisionEvents.Sort();
                _presentationNetEventsHandler.ProcessPlayerToEnvironmentTeleportCollisionEvents(_cachedUnprocessedPlayerToEnvironmentTeleportCollisionEvents);
            }
        }

        private void ProcessPreparationPhaseEndedEvents(FixedUnorderedList<PreparationPhaseEndedNetEventS2C> preparationPhaseEndedNetEvents)
        {
            _cachedUnprocessedPreparationPhaseEndedEvents.Clear();

            foreach (var netEvent in preparationPhaseEndedNetEvents.AsSpan())
            {
                if (netEvent.OccuredOnTick > LastProcessedTickFromServer)
                {
                    _cachedUnprocessedPreparationPhaseEndedEvents.Add(netEvent);
                }
            }

            if (!_cachedUnprocessedPreparationPhaseEndedEvents.IsNullOrEmpty())
            {
                _cachedUnprocessedPreparationPhaseEndedEvents.Sort();
                _presentationNetEventsHandler.ProcessPreparationPhaseEndedEvents(_cachedUnprocessedPreparationPhaseEndedEvents);
            }
        }

        private void UpdateRotatingWheels(int tick)
        {
            if (_matchDataService.IsInPreparationPhase)
            {
                return;
            }
            
            var calculationTick = tick - _matchDataService.StartPhaseInitialTick;
            var deltaTime = _networkConfig.DeltaTime;

            foreach (var wheelModel in _matchDataService.RotatingWheels)
            {
                var wheelCenter = wheelModel.CenterPosition;
                var rotationSpeed = wheelModel.RotationSpeed;

                foreach (var wallId in wheelModel.WallIds)
                {
                    var wallModel = _matchDataService.GetEnvironmentWall(wallId);

                    EnvironmentRotatingWheelUtils.CalculateChildTransform(
                        calculationTick, rotationSpeed, deltaTime, wheelCenter, wallModel.LocalPosition, 0,
                        out var worldPos, out var worldRot
                    );

                    wallModel.WorldPosition = worldPos;
                    wallModel.WorldRotationAngle = worldRot;
                }


                foreach (var lavaWallId in wheelModel.LavaWallIds)
                {
                    var lavaWallModel = _matchDataService.GetEnvironmentLavaWall(lavaWallId);

                    EnvironmentRotatingWheelUtils.CalculateChildTransform(
                        calculationTick, rotationSpeed, deltaTime, wheelCenter, lavaWallModel.LocalPosition, 0,
                        out var worldPos, out var worldRot
                    );

                    lavaWallModel.WorldPosition = worldPos;
                    lavaWallModel.WorldRotationAngle = worldRot;
                }


                foreach (var springId in wheelModel.SpringIds)
                {
                    var springModel = _matchDataService.GetEnvironmentSpring(springId);

                    EnvironmentRotatingWheelUtils.CalculateChildTransform(
                        calculationTick, rotationSpeed, deltaTime, wheelCenter, springModel.LocalPosition, springModel.LocalRotationAngle,
                        out var worldPos, out var worldRot
                    );

                    springModel.WorldPosition = worldPos;
                    springModel.WorldRotationAngle = worldRot;
                }


                foreach (var pairId in wheelModel.TeleportGatePairIds)
                {
                    var teleportPairModel = _matchDataService.GetTeleportPair(pairId);

                    EnvironmentRotatingWheelUtils.CalculateChildTransform(
                        calculationTick, rotationSpeed, deltaTime, wheelCenter, teleportPairModel.GateA.LocalPosition, teleportPairModel.GateA.LocalRotation,
                        out var worldPosA, out var worldRotA
                    );

                    teleportPairModel.GateA.WorldPosition = worldPosA;
                    teleportPairModel.GateA.WorldRotation = worldRotA;

                    EnvironmentRotatingWheelUtils.CalculateChildTransform(
                        calculationTick, rotationSpeed, deltaTime, wheelCenter, teleportPairModel.GateB.LocalPosition, teleportPairModel.GateB.LocalRotation,
                        out var worldPosB, out var worldRotB
                    );

                    teleportPairModel.GateB.WorldPosition = worldPosB;
                    teleportPairModel.GateB.WorldRotation = worldRotB;
                }
            }
        }

        private void UpdatePowerUpBallsTransform(MatchSimulationStateS2C simulationState)
        {
            foreach (var powerUpBallModel in _matchDataService.PowerUpBalls)
            {
                var powerUpBallById = simulationState.GetPowerUpBallById(powerUpBallModel.Id);
                powerUpBallModel.Position = powerUpBallById.Position.ToUnityVector2();
            }
        }

        private void ProcessTalentCardHitEvents(FixedUnorderedList<TalentCardHitNetEventS2C> talentCardHitNetEvents)
        {
            _cachedUnprocessedTalentCardHitEvents.Clear();

            foreach (var netEvent in talentCardHitNetEvents.AsSpan())
            {
                if (netEvent.OccuredOnTick > LastProcessedTickFromServer)
                {
                    _cachedUnprocessedTalentCardHitEvents.Add(netEvent);
                }
            }

            if (!_cachedUnprocessedTalentCardHitEvents.IsNullOrEmpty())
            {
                _cachedUnprocessedTalentCardHitEvents.Sort();
                _presentationNetEventsHandler.ProcessTalentCardHitEvents(_cachedUnprocessedTalentCardHitEvents);
            }
        }

        private void ProcessTalentCardObtainedEvents(FixedClassUnorderedList<TalentCardObtainedNetEventS2C> talentCardObtainedNetEvents)
        {
            _cachedUnprocessedTalentCardObtainedEvents.Clear();

            foreach (var netEvent in talentCardObtainedNetEvents.AsSpan())
            {
                if (netEvent.OccuredOnTick > LastProcessedTickFromServer)
                {
                    _cachedUnprocessedTalentCardObtainedEvents.Add(netEvent);
                }
            }

            if (!_cachedUnprocessedTalentCardObtainedEvents.IsNullOrEmpty())
            {
                _cachedUnprocessedTalentCardObtainedEvents.Sort();
                _presentationNetEventsHandler.ProcessTalentCardObtainedEvents(_cachedUnprocessedTalentCardObtainedEvents);
            }
        }

        private void ProcessPlayerSwapEvents(FixedUnorderedList<PlayersSwapNetEventS2C> playerSwapNetEvents)
        {
            _cachedUnprocessedPlayerSwapEvents.Clear();

            foreach (var netEvent in playerSwapNetEvents.AsSpan())
            {
                if (netEvent.OccuredOnTick > LastProcessedTickFromServer)
                {
                    _cachedUnprocessedPlayerSwapEvents.Add(netEvent);
                }
            }

            if (!_cachedUnprocessedPlayerSwapEvents.IsNullOrEmpty())
            {
                _cachedUnprocessedPlayerSwapEvents.Sort();
                _presentationNetEventsHandler.ProcessPlayerSwapEvents(_cachedUnprocessedPlayerSwapEvents);
            }
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
                _cachedUnprocessedBulletDestroyedEvents.Sort();
                _presentationNetEventsHandler.ProcessBulletDestroyedEvents(_cachedUnprocessedBulletDestroyedEvents);
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
                _cachedUnprocessedPlayerTakeDamageEvents.Sort();
                _presentationNetEventsHandler.ProcessPlayerTakeDamageEvents(_cachedUnprocessedPlayerTakeDamageEvents);
            }
        }

        private void ProcessPlayerDiedEvents(FixedUnorderedList<PlayerDiedNetEventS2C> playerDiedNetEvents)
        {
            _cachedUnprocessedPlayerDiedEvents.Clear();

            foreach (var netEvent in playerDiedNetEvents.AsSpan())
            {
                if (netEvent.OccuredOnTick > LastProcessedTickFromServer)
                {
                    _cachedUnprocessedPlayerDiedEvents.Add(netEvent);
                }
            }

            if (!_cachedUnprocessedPlayerDiedEvents.IsNullOrEmpty())
            {
                _cachedUnprocessedPlayerDiedEvents.Sort();
                _presentationNetEventsHandler.ProcessPlayerDiedEvents(_cachedUnprocessedPlayerDiedEvents);
            }
        }


        private void ProcessPlayerRejoinedEvents(FixedClassUnorderedList<PlayerRejoinAcceptPacketS2C> playerRejoinAcceptNetEvents)
        {
            _cachedUnprocessedPlayerRejoinedEvents.Clear();

            foreach (var netEvent in playerRejoinAcceptNetEvents.AsSpan())
            {
                if (netEvent.OccuredOnTick > LastProcessedTickFromServer)
                {
                    _cachedUnprocessedPlayerRejoinedEvents.Add(netEvent);
                }
            }

            if (!_cachedUnprocessedPlayerRejoinedEvents.IsNullOrEmpty())
            {
                _cachedUnprocessedPlayerRejoinedEvents.Sort();
                _presentationNetEventsHandler.ProcessPlayerRejoinedEvents(_cachedUnprocessedPlayerRejoinedEvents);
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
                _cachedUnprocessedBulletSpawnedEvents.Sort();
                _presentationNetEventsHandler.ProcessBulletSpawnEvents(_cachedUnprocessedBulletSpawnedEvents);
            }
        }
        
        private void ProcessPowerUpBallSpawnedEvents(FixedUnorderedList<PowerUpBallSpawnedNetEventS2C> powerUpBallSpawnNetEvents)
        {
            _cachedUnprocessedPowerUpBallSpawnedEvents.Clear();

            foreach (var netEvent in powerUpBallSpawnNetEvents.AsSpan())
            {
                if (netEvent.OccuredOnTick > LastProcessedTickFromServer)
                {
                    _cachedUnprocessedPowerUpBallSpawnedEvents.Add(netEvent);
                }
            }

            if (!_cachedUnprocessedPowerUpBallSpawnedEvents.IsNullOrEmpty())
            {
                _cachedUnprocessedPowerUpBallSpawnedEvents.Sort();
                _presentationNetEventsHandler.ProcessPowerUpSpawnedEvents(_cachedUnprocessedPowerUpBallSpawnedEvents);
            }
        }
        
        private void ProcessPowerUpBallObtainedEvents(FixedUnorderedList<PowerUpBallObtainedNetEventS2C> powerUpBallObtainedNetEvents)
        {
            _cachedUnprocessedPowerUpBallObtainedEvents.Clear();

            foreach (var netEvent in powerUpBallObtainedNetEvents.AsSpan())
            {
                if (netEvent.OccuredOnTick > LastProcessedTickFromServer)
                {
                    _cachedUnprocessedPowerUpBallObtainedEvents.Add(netEvent);
                }
            }

            if (!_cachedUnprocessedPowerUpBallObtainedEvents.IsNullOrEmpty())
            {
                _cachedUnprocessedPowerUpBallObtainedEvents.Sort();
                _presentationNetEventsHandler.ProcessPowerUpObtainedEvents(_cachedUnprocessedPowerUpBallObtainedEvents);
            }
        }

        private void ProcessTeamLostEvents(FixedUnorderedList<TeamLostNetEventS2C> teamLostNetEvents)
        {
            _cachedUnprocessedTeamLostEvents.Clear();

            foreach (var netEvent in teamLostNetEvents.AsSpan())
            {
                if (netEvent.OccuredOnTick > LastProcessedTickFromServer)
                {
                    _cachedUnprocessedTeamLostEvents.Add(netEvent);
                }
            }

            if (!_cachedUnprocessedTeamLostEvents.IsNullOrEmpty())
            {
                _cachedUnprocessedTeamLostEvents.Sort();
                _presentationNetEventsHandler.ProcessTeamLostEvents(_cachedUnprocessedTeamLostEvents);
            }
        }

        private void ProcessTalentSwitchEvents(FixedUnorderedList<TalentSwitchNetEventS2C> talentSwitchNetEvents)
        {
            _cachedUnprocessedTalentSwitchEvents.Clear();

            foreach (var netEvent in talentSwitchNetEvents.AsSpan())
            {
                if (netEvent.OccuredOnTick > LastProcessedTickFromServer)
                {
                    _cachedUnprocessedTalentSwitchEvents.Add(netEvent);
                }
            }

            if (!_cachedUnprocessedTalentSwitchEvents.IsNullOrEmpty())
            {
                _cachedUnprocessedTalentSwitchEvents.Sort();
                _presentationNetEventsHandler.ProcessTalentSwitchEvents(_cachedUnprocessedTalentSwitchEvents);
            }
        }

        private void ProcessEnvironmentSpringPlayerCollisionEvents(FixedUnorderedList<EnvironmentSpringPlayerCollisionNetEventS2C> environmentSpringPlayerCollisionNetEvents)
        {
            _cachedUnprocessedEnvironmentSpringPlayerCollisionEvents.Clear();

            foreach (var netEvent in environmentSpringPlayerCollisionNetEvents.AsSpan())
            {
                if (netEvent.OccuredOnTick > LastProcessedTickFromServer)
                {
                    _cachedUnprocessedEnvironmentSpringPlayerCollisionEvents.Add(netEvent);
                }
            }

            if (!_cachedUnprocessedEnvironmentSpringPlayerCollisionEvents.IsNullOrEmpty())
            {
                _cachedUnprocessedEnvironmentSpringPlayerCollisionEvents.Sort();
                _presentationNetEventsHandler.ProcessEnvironmentSpringPlayerCollisionEvents(_cachedUnprocessedEnvironmentSpringPlayerCollisionEvents);
            }
        }

        private void ProcessStageEndEvents(FixedClassUnorderedList<StageEndNetEventS2C> stageEndNetEvents)
        {
            _cachedUnprocessedStageEndEvents.Clear();

            foreach (var netEvent in stageEndNetEvents.AsSpan())
            {
                if (netEvent.OccuredOnTick > LastProcessedTickFromServer)
                {
                    _cachedUnprocessedStageEndEvents.Add(netEvent);
                }
            }

            if (!_cachedUnprocessedStageEndEvents.IsNullOrEmpty())
            {
                _cachedUnprocessedStageEndEvents.Sort();
                _presentationNetEventsHandler.ProcessStageEndEvents(_cachedUnprocessedStageEndEvents);
            }
        }

        private void ProcessGainBoltsEvents(FixedUnorderedList<GainBoltsNetEventS2C> gainBoltsNetEvents)
        {
            _cachedUnprocessedGainBoltsEvents.Clear();

            foreach (var netEvent in gainBoltsNetEvents.AsSpan())
            {
                if (netEvent.OccuredOnTick > LastProcessedTickFromServer)
                {
                    _cachedUnprocessedGainBoltsEvents.Add(netEvent);
                }
            }

            if (!_cachedUnprocessedGainBoltsEvents.IsNullOrEmpty())
            {
                _cachedUnprocessedGainBoltsEvents.Sort();
                _presentationNetEventsHandler.ProcessGainBoltsNetEvents(_cachedUnprocessedGainBoltsEvents);
            }
        }

        private void UpdatePlayersDeltas(MatchSimulationStateS2C simulationState)
        {
            foreach (var playerModel in _matchDataService.Players)
            {
                var playerState = simulationState.GetPlayerById(playerModel.PlayerId);
                playerModel.Spaceship.Transform.Position = playerState.Spaceship.Transform.Position;
                playerModel.Spaceship.Transform.Direction = playerState.Spaceship.Transform.Direction;
                playerModel.Spaceship.Shoot.CooldownSecondsLeft = playerState.Spaceship.Shoot.CooldownSecondsLeft;
                playerModel.Spaceship.TalentsState.AimDirection = playerState.Spaceship.TalentsState.AimDirection;

                var sourceTalents = playerState.Spaceship.TalentsState.Talents;
                var destinationTalents = playerModel.Spaceship.TalentsState.Talents;
                var talentsAmount = destinationTalents.Count;

                if (sourceTalents.Count != destinationTalents.Count)
                {
                    LogService.LogError($"For some reason there's a different amount of talents for player {playerModel.PlayerId} in state {sourceTalents.Count} and in presentation {destinationTalents.Count}");
                    talentsAmount =  System.Math.Min(sourceTalents.Count, destinationTalents.Count);
                }
                

            }
        }

        private void UpdateBulletsTransform(MatchSimulationStateS2C simulationState)
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
        
        private void OnFullTickReceived(MatchFullTickPacketS2C fullTickPacket)
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