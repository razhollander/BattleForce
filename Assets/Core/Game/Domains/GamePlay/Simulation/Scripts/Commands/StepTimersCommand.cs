using Core.Game.Domains.GamePlay.Simulation.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Physics;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.Commands
{
    public class StepTimersCommand : BaseCommand, ICommandVoid
    {
        private IMatchDataService _matchDataService;
        private IPowerUpsSpawnerService _powerUpsSpawnerService;
        private IPlayersInLavaTrackerService _playersInLavaTrackerService;
        private float _deltaTime;

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
        }

        public void Execute()
        {
            StepPlayersShootCooldown(_deltaTime);
            StepPlayersTalentsCooldowns(_deltaTime);
            _powerUpsSpawnerService.StepTimer(_deltaTime);
            _playersInLavaTrackerService.StepTimePassedSinceLastDamageTaken(_deltaTime);
        }

        private void StepPlayersTalentsCooldowns(float deltaTime)
        {
            foreach (var playerState in _matchDataService.SimulationState.Players.AsSpan())
            {
                for (int i = 0; i < playerState.Spaceship.Talents.Talents.Count; i++)
                {
                    var playerTalent = playerState.Spaceship.Talents.Talents[i];
                    var isCurrentlyOnCooldown = playerTalent.CooldownSecondsLeft < playerTalent.MaxCooldown;

                    if (isCurrentlyOnCooldown)
                    {
                        playerTalent.CooldownSecondsLeft -= deltaTime;
                    }

                    if (playerTalent.CooldownSecondsLeft < 0)
                    {
                        playerTalent.CooldownSecondsLeft = playerTalent.MaxCooldown;
                    }

                    playerState.Spaceship.Talents.Talents[i] = playerTalent;
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