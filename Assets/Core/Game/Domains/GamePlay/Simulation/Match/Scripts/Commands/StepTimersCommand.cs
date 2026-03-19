using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.PlayersInLavaTracker;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.PowerUpsSpawner;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Stage;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Controllers;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Commands
{
    public class StepTimersCommand : BaseCommand, ICommandVoid
    {
        private IMatchDataService _matchDataService;
        private IPowerUpsSpawnerService _powerUpsSpawnerService;
        private IPlayersInLavaTrackerService _playersInLavaTrackerService;
        private IHeadLessQuitterController _headLessQuitterController;
        private IStageDataService _stageDataService;
        private IPreparationPhaseTimerService _preparationPhaseTimerService;
        
        private float _deltaTime;
        private int _tick;

        public StepTimersCommand SetStepTick(int tick)
        {
            _tick = tick;
            return this;
        }
        
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
            _headLessQuitterController = _diContainer.Resolve<IHeadLessQuitterController>();
            _preparationPhaseTimerService = _diContainer.Resolve<IPreparationPhaseTimerService>();
            _stageDataService = _diContainer.Resolve<IStageDataService>();
        }

        public void Execute()
        {
            StepPlayersShootCooldown(_deltaTime);
            ResetPlayersTalentsCooldownsIfEnded();
            _powerUpsSpawnerService.StepTimer(_deltaTime);
            _playersInLavaTrackerService.StepTimePassedSinceLastDamageTaken(_deltaTime);
            _headLessQuitterController.StepTimer(_deltaTime);
            StepPreperationPhaseTimer();
        }

        private void StepPreperationPhaseTimer()
        {
            if (!_matchDataService.SimulationState.IsInPreparationPhase)
            {
                return;
            }
            
            _preparationPhaseTimerService.StepPreperationPhaseTimer(_deltaTime);
        }

        private void ResetPlayersTalentsCooldownsIfEnded()
        {
            foreach (var playerState in _matchDataService.SimulationState.Players.AsSpan())
            {
                for (int i = 0; i < playerState.Spaceship.TalentsState.Talents.Count; i++)
                {
                    var playerTalent = playerState.Spaceship.TalentsState.Talents[i];
                    if (!playerTalent.IsOnCooldown())
                    {
                        continue;
                    }

                    var didCooldownEnd = playerTalent.CooldownEndTick <= _tick;
                    if (!didCooldownEnd)
                    {
                        continue;
                    }

                    playerTalent.CooldownEndTick = 0;
                    playerState.Spaceship.TalentsState.Talents[i] = playerTalent;
                }
            }
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