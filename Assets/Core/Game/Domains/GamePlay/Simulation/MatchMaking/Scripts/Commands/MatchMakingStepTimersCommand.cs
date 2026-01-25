using Core.Game.Domains.GamePlay.Simulation.Scripts.MatchMakingModel.MatchMakingModel;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.MatchMakingModel.Commands
{
    public class MatchMakingStepTimersCommand : BaseCommand, ICommandVoid
    {
        private IMatchMakingDataService _matchMakingDataService;
        private float _deltaTime;

        public MatchMakingStepTimersCommand SetStepDeltaTime(float deltaTime)
        {
            _deltaTime = deltaTime;

            return this;
        }

        public override void ResolveDependencies()
        {
            _matchMakingDataService = _diContainer.Resolve<IMatchMakingDataService>();
        }

        public void Execute()
        {
            StepPlayersShootCooldown(_deltaTime);
        }

        private void StepPlayersShootCooldown(float deltaTime)
        {
            foreach (var playerState in _matchMakingDataService.SimulationState.Players.AsSpan())
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