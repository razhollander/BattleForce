using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.PlayersInLavaTracker;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.PlayersTouchingSpikesTracker;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.PowerUpsSpawner;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Controllers;
using CoreDomain.Scripts.Services.CommandFactory;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.PlayerLockOnTarget;
using Core.Scripts.Network;
using Core.Scripts.Utils.CustomCollections;
using CoreDomain.Scripts.Services.Logger.Base;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Commands
{
    public class StepTimersCommand : BaseCommand, ICommandVoid
    {
        private IMatchDataService _matchDataService;
        private IPowerUpsSpawnerService _powerUpsSpawnerService;
        private IPlayersInLavaTrackerService _playersInLavaTrackerService;
        private IPlayersTouchingSpikesTrackerService _playersTouchingSpikesTrackerService;
        private IHeadLessQuitterController _headLessQuitterController;
        private IPreparationPhaseTimerService _preparationPhaseTimerService;
        private ILockOnTargetTimerService _lockOnTargetTimerService;
        
        private float _deltaTime;
        private FixedUnorderedList<ushort> _cachedPlayerIdsNotToIncrementTimerInLavaList;

        public StepTimersCommand SetStepDeltaTime(float deltaTime)
        {
            _deltaTime = deltaTime;
            return this;
        }

        public override void ResolveDependencies()
        {
            _matchDataService = _diContainer.Resolve<IMatchDataService>();
            _powerUpsSpawnerService = _diContainer.Resolve<IPowerUpsSpawnerService>();
            _playersInLavaTrackerService = _diContainer.Resolve<IPlayersInLavaTrackerService>();
            _playersTouchingSpikesTrackerService = _diContainer.Resolve<IPlayersTouchingSpikesTrackerService>();
            _headLessQuitterController = _diContainer.Resolve<IHeadLessQuitterController>();
            _preparationPhaseTimerService = _diContainer.Resolve<IPreparationPhaseTimerService>();
            _lockOnTargetTimerService = _diContainer.Resolve<ILockOnTargetTimerService>();
            var networkConfig = _diContainer.Resolve<NetworkConfig>();
            _cachedPlayerIdsNotToIncrementTimerInLavaList = new FixedUnorderedList<ushort>(networkConfig.MaxCap.ConcurrentPlayers);
        }

        public void Execute()
        {
            StepPlayersShootCooldown(_deltaTime);
            _powerUpsSpawnerService.StepTimer(_deltaTime);
            StepPlayersInHazardsTimers(_deltaTime);
            _headLessQuitterController.StepTimer(_deltaTime);
            StepPreperationPhaseTimer(_deltaTime);
            _lockOnTargetTimerService.StepTimers(_deltaTime);
        }

        // Rock/Frozen players are immune to hazard damage, so their damage-interval timers must not advance while immune.
        private void StepPlayersInHazardsTimers(float deltaTime)
        {
            var immunePlayerIds = GetRockOrFrozenPlayerIds();
            _playersInLavaTrackerService.StepTimePassedSinceLastDamageTaken(immunePlayerIds, deltaTime);
            _playersTouchingSpikesTrackerService.StepTimePassedSinceLastDamageTaken(immunePlayerIds, deltaTime);
        }

        private FixedUnorderedList<ushort> GetRockOrFrozenPlayerIds()
        {
            _cachedPlayerIdsNotToIncrementTimerInLavaList.Clear();
            foreach (var playerState in _matchDataService.SimulationState.Players.AsSpan())
            {
                var IsPlayerRockOrFrozen = playerState.Spaceship.TalentsState.TryGetCurrentSelectedTalent(out var selectedTalent) &&
                                                   selectedTalent is {IsCurrentlyActive: true, TalentType: TalentType.Rock or TalentType.Frozen};
                if (IsPlayerRockOrFrozen)
                {
                    ref var playerId = ref _cachedPlayerIdsNotToIncrementTimerInLavaList.AddAndGet();
                    playerId = playerState.Id;
                }
            }

            return _cachedPlayerIdsNotToIncrementTimerInLavaList;
        }

        private void StepPreperationPhaseTimer(float deltaTime)
        {
            if (!_matchDataService.SimulationState.IsInPreparationPhase)
            {
                return;
            }
            
            _preparationPhaseTimerService.StepPreperationPhaseTimer(deltaTime);
        }

        private void StepPlayersShootCooldown(float deltaTime)
        {
            foreach (var playerState in _matchDataService.SimulationState.Players.AsSpan())
            {
                var shootState = playerState.Spaceship.Shoot;
                var isCurrentlyOnCooldown = shootState.CooldownSecondsLeft < shootState.MaxCooldown;
                if (isCurrentlyOnCooldown)
                {
                    shootState.CooldownSecondsLeft -= deltaTime;
                }
                
                if (shootState.CooldownSecondsLeft < 0)
                {
                    shootState.CooldownSecondsLeft = shootState.MaxCooldown;
                }
                
                playerState.Spaceship.Shoot = shootState;
            }
        }
    }
}