using Core.Game.Domains.GamePlay.Simulation.Scripts.Services.GamePlayConfig;
using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.OverrideableNetEvents;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.PlayersInLavaTracker;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Inputs;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Physics;
using Core.Scripts.Network;
using Core.Scripts.Utils;
using CoreDomain.Scripts.Services.CommandFactory;
using CoreDomain.Scripts.Services.Logger.Base;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Talent
{
    public class PlayersTalentsManager : IPlayersTalentsManager
    {
        private readonly IMatchDataService _matchDataService;
        private readonly SharedGamePlayConfig _sharedGamePlayConfig;
        private readonly ISimulationGamePlayConfigService _gamePlayConfigService;
        private readonly Dictionary<int, PlayerTalentControllers> _talentControllersPerPlayer;
        private readonly ConcurrentPool<PlayerTalentControllers> _talentControllersPool;

        public PlayersTalentsManager(NetworkConfig networkConfig, IMatchDataService matchDataService, SharedGamePlayConfig sharedGamePlayConfig,
            INetEventsDataService netEventsDataService, ISimulationGamePlayConfigService gamePlayConfigService, IPhysicsSimulator physicsSimulator,
            IOverrideableNetEventsService overrideableNetEventsService, ICommandFactory commandFactory, IPlayersMouseDataService playersMouseDataService, IPlayersInLavaTrackerService playersInLavaTrackerService)
        {
            _matchDataService = matchDataService;
            _sharedGamePlayConfig = sharedGamePlayConfig;
            _gamePlayConfigService = gamePlayConfigService;
            _talentControllersPerPlayer = new Dictionary<int, PlayerTalentControllers>(networkConfig.MaxCap.ConcurrentPlayers);

            _talentControllersPool = new ConcurrentPool<PlayerTalentControllers>(
                () => new PlayerTalentControllers(netEventsDataService, matchDataService, gamePlayConfigService, physicsSimulator, networkConfig, overrideableNetEventsService,
                    commandFactory, sharedGamePlayConfig, playersMouseDataService, playersInLavaTrackerService), networkConfig.MaxCap.ConcurrentPlayers);
        }

        public void AddPlayer(ushort playerId)
        {
            var playerTalentControllers = _talentControllersPool.Get();
            playerTalentControllers.SetCasterId(playerId);
            playerTalentControllers.InitEntryPoint();
            _talentControllersPerPlayer.Add(playerId, playerTalentControllers);
        }

        public void RemovePlayer(ushort playerId)
        {
            _talentControllersPool.Return(_talentControllersPerPlayer[playerId]);
            _talentControllersPerPlayer.Remove(playerId);
        }

        public bool TryAddTalentToPlayer(TalentType talentType, ushort playerId, int tick, out TalentStateS2C newTalent, out bool didReplaceExistingTalent)
        {
            var playerState = _matchDataService.SimulationState.GetPlayerById(playerId);
            didReplaceExistingTalent = false;

            if (DoesPlayerHaveTalent(playerState, talentType))
            {
                newTalent = default;
                return false;
            }
            
            var didPlayerReachMaxTalents = playerState.Spaceship.TalentsState.Talents.Count == _sharedGamePlayConfig.MaxConcurrentTalentsForPlayer;
            if (didPlayerReachMaxTalents)
            {
                newTalent = ReplaceTalentWithCurrentSelectedTalent(talentType, playerState, tick);
                didReplaceExistingTalent = true;
            }
            else
            {
                newTalent = AddTalentToPlayer(talentType, playerState);
            }
            
            return true;
        }

        private bool DoesPlayerHaveTalent(PlayerStateS2C playerState, TalentType talentType)
        {
            foreach (var talentState in playerState.Spaceship.TalentsState.Talents.AsSpan())
            {
                if (talentType == talentState.TalentType)
                {
                    return true;
                }
            }

            return false;
        }

        public bool TrySwitchToNextTalent(ushort playerId)
        {
            var playerState = _matchDataService.SimulationState.GetPlayerById(playerId);
            var talents = playerState.Spaceship.TalentsState;

            if (!talents.TryGetCurrentSelectedTalent(out var selectedTalent))
            {
                return false;
            }

            if (selectedTalent.IsCurrentlyActive)
            {
                return false;
            }

            talents.SelectedTalentIndex++;
            if (talents.SelectedTalentIndex >= talents.Talents.Count)
            {
                talents.SelectedTalentIndex = 0;
            }

            return true;
        }

        public bool TrySwitchToTalent(ushort playerId, int talentIndex, int tick)
        {
            var playerState = _matchDataService.SimulationState.GetPlayerById(playerId);
            var talents = playerState.Spaceship.TalentsState;

            if (!talents.TryGetCurrentSelectedTalent(out var selectedTalent))
            {
                return false;
            }

            if (talentIndex >= talents.Talents.Count)
            {
                LogService.LogError($"Tryed to switch to talent index {talentIndex} which is out of range of {talents.Talents.Count} talents");
                return false;
            }

            if (selectedTalent.TalentType == TalentType.Headbutt)
            {
                _talentControllersPerPlayer[playerId].StopTalentIfActive(TalentType.Headbutt, tick);
            }
            else if (selectedTalent.TalentType == TalentType.Frozen)
            {
                _talentControllersPerPlayer[playerId].StopTalentIfActive(TalentType.Frozen, tick);
            }
            else if (selectedTalent.TalentType == TalentType.Rock)
            {
                _talentControllersPerPlayer[playerId].StopTalentIfActive(TalentType.Rock, tick);
            }
            else if (selectedTalent.IsCurrentlyActive)
            {
                return false;
            }

            talents.SelectedTalentIndex = talentIndex;
            return true;
        }

        public void ProcessPlayerTalentInput(ushort playerId, TalentType talentType, int tick, bool wasTalentInputDownThisTick, bool isTalentInputPressed, bool wasTalentInputReleasedThisTick,
            float deltaTime)
        {
            _talentControllersPerPlayer[playerId].ProcessTalentInput(talentType, wasTalentInputDownThisTick, isTalentInputPressed, wasTalentInputReleasedThisTick, tick, deltaTime);
        }

        public void ProcessAllTalentsTickOfPlayer(ushort playerId, int tick, float deltaTime)
        {
            _talentControllersPerPlayer[playerId].OnTick(tick, deltaTime);
        }

        public void CompleteSwapTalentWithEnemy(ushort casterId, ushort enemyPlayerId, int tick)
        {
            if (_talentControllersPerPlayer.TryGetValue(casterId, out var controllers))
            {
                controllers.CompleteSwapTalentWithEnemy(enemyPlayerId, tick);
            }
            else
            {
                LogService.LogError($"No caster found for player id {casterId}");
            }
        }

        public void ResetAllTalentsData()
        {
            foreach (var kvp in _talentControllersPerPlayer)
            {
                kvp.Value.ResetData();
            }
        }

        public void HitKOTalentWithEnemy(ushort casterId, ushort enemyPlayerId, int tick)
        {
            if (_talentControllersPerPlayer.TryGetValue(casterId, out var controllers))
            {
                controllers.HitKOTalentWithEnemy(enemyPlayerId, tick);
            }
            else
            {
                LogService.LogError($"No caster found for player id {casterId}");
            }
        }

        public void HitKOTalentWithWall(ushort casterId)
        {
            if (_talentControllersPerPlayer.TryGetValue(casterId, out var controllers))
            {
                controllers.HitKOTalentWithWall();
            }
            else
            {
                LogService.LogError($"No caster found for player id {casterId}");
            }
        }

        public void HitKOTalentWithMole(ushort casterId, ushort moleId, int tick)
        {
            if (_talentControllersPerPlayer.TryGetValue(casterId, out var controllers))
            {
                controllers.HitKOTalentWithMole(moleId, tick);
            }
            else
            {
                LogService.LogError($"No caster found for player id {casterId}");
            }
        }

        public void HitGrapplingHook(ushort casterId, ushort projectileId, GrapplingHookHitType hitType, ushort attachedEntityId, int tick)
        {
            if (_talentControllersPerPlayer.TryGetValue(casterId, out var controllers))
            {
                controllers.HitGrapplingHook(hitType, attachedEntityId, tick);
            }
            else
            {
                LogService.LogError($"No caster found for player id {casterId}");
            }
        }

        public void CatchFishingRodWithEnemy(ushort casterId, ushort enemyPlayerId, int tick)
        {
            if (_talentControllersPerPlayer.TryGetValue(casterId, out var controllers))
            {
                controllers.CatchFishingRodEnemy(enemyPlayerId, tick);
            }
            else
            {
                LogService.LogError($"No caster found for player id {casterId}");
            }
        }

        public void CatchFishingRodWithMole(ushort casterId, ushort moleId, int tick)
        {
            if (_talentControllersPerPlayer.TryGetValue(casterId, out var controllers))
            {
                controllers.CatchFishingRodMole(moleId, tick);
            }
            else
            {
                LogService.LogError($"No caster found for player id {casterId}");
            }
        }

        public void HitFishingRodWithWall(ushort casterId, ushort projectileId, int tick)
        {
            if (_talentControllersPerPlayer.TryGetValue(casterId, out var controllers))
            {
                controllers.HitFishingRodWithWall(tick);
            }
            else
            {
                LogService.LogError($"No caster found for player id {casterId}");
            }
        }

        public void HitSoulGhostWithWall(ushort casterId, ushort ghostId, int tick)
        {
            if (_talentControllersPerPlayer.TryGetValue(casterId, out var controllers))
            {
                controllers.HitSoulGhostWithWall(tick);
            }
            else
            {
                LogService.LogError($"No caster found for player id {casterId}");
            }
        }

        public void StopTalentIfActive(TalentType talentType, ushort playerId, int tick)
        {
            _talentControllersPerPlayer[playerId].StopTalentIfActive(talentType, tick);
        }

        public void TryHeadbuttHitEnemy(ushort potentialCasterId, ushort potentialEnemyId, int tick)
        {
            var casterState = _matchDataService.SimulationState.GetPlayerById(potentialCasterId);
            var enemyState = _matchDataService.SimulationState.GetPlayerById(potentialEnemyId);
            var arePlayersEnemies = casterState.TeamId != enemyState.TeamId;

            if (!arePlayersEnemies)
            {
                return;
            }

            if (_talentControllersPerPlayer.TryGetValue(potentialCasterId, out var controllers))
            {
                controllers.TryHeadbuttHitEnemy(potentialCasterId, potentialEnemyId, tick);
            }
        }

        public void HeadbuttHitMole(ushort casterId)
        {
            if (_talentControllersPerPlayer.TryGetValue(casterId, out var controllers))
            {
                controllers.HeadbuttHitMole();
            }
            else
            {
                LogService.LogError($"No caster found for player id {casterId}");
            }
        }

        public bool IsHeadbuttCharging(ushort playerId)
        {
            return _talentControllersPerPlayer[playerId].IsHeadbuttCharging();
        }

        private TalentStateS2C AddTalentToPlayer(TalentType talentType, PlayerStateS2C playerState)
        {
            ref var newTalent = ref playerState.Spaceship.TalentsState.Talents.AddAndGet();
            newTalent.Setup(talentType);
            SetupTalentCooldown(ref newTalent);
            return newTalent;
        }

        private void SetupTalentCooldown(ref TalentStateS2C newTalent)
        {
            var talentType = newTalent.TalentType;
            var cooldownConfig = _gamePlayConfigService.GamePlayConfig.Talents.TalentsCooldownsConfigs.TalentCooldownConfigs.Find(x => x.TalentType == talentType);

            switch (cooldownConfig.CooldownType)
            {
                case TalentCooldownType.Normal:
                    var normalCooldownConfig = (TalentNormalCooldownConfig) cooldownConfig;
                    newTalent.SetupWithNormalCooldown(normalCooldownConfig.CooldownInSeconds);
                    break;
                case TalentCooldownType.Stocks:
                    var stocksCooldownConfig = (TalentStocksCooldownConfig) cooldownConfig;
                    newTalent.SetupWithStocksCooldown(stocksCooldownConfig.MaxStocks, stocksCooldownConfig.SingleStockCooldownInSeconds);
                    break;
                case TalentCooldownType.AlwaysActive:
                    newTalent.SetupWithAlwaysActive();
                    break;
            }
        }

        private TalentStateS2C ReplaceTalentWithCurrentSelectedTalent(TalentType talentType, PlayerStateS2C playerState, int tick)
        {
            ref var currentSelectedTalent = ref playerState.Spaceship.TalentsState.Talents.Get(playerState.Spaceship.TalentsState.SelectedTalentIndex);
            _talentControllersPerPlayer[playerState.Id].StopTalentIfActive(currentSelectedTalent.TalentType, tick);
            currentSelectedTalent.Setup(talentType);
            SetupTalentCooldown(ref currentSelectedTalent);
            return currentSelectedTalent;
        }
    }
}