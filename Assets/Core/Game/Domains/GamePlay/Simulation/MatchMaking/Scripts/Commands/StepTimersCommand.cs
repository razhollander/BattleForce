using Core.Game.Domains.GamePlay.Simulation.MatchMaking.Scripts.PlayerLockOnWall;
using Core.Game.Domains.GamePlay.Simulation.MatchMaking.Scripts.StartMatchWall;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Controllers;
using Core.Game.Domains.GamePlay.Simulation.Scripts.MatchMakingModel.MatchMakingModel;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Simulation.MatchMaking.Scripts.Commands
{
    public class StepTimersCommand : BaseCommand, ICommandVoid
    {
        private IMatchMakingDataService _matchMakingDataService;
        private IStartMatchWallController _startMatchWallController;
        private IHeadLessQuitterController _headLessQuitterController;
        private ILockOnWallTimerService _lockOnWallTimerService;

        private float _deltaTime;

        public StepTimersCommand SetStepDeltaTime(float deltaTime)
        {
            _deltaTime = deltaTime;
            return this;
        }

        public override void ResolveDependencies()
        {
            _matchMakingDataService = _diContainer.Resolve<IMatchMakingDataService>();
            _startMatchWallController = _diContainer.Resolve<IStartMatchWallController>();
            _headLessQuitterController = _diContainer.Resolve<IHeadLessQuitterController>();
            _lockOnWallTimerService = _diContainer.Resolve<ILockOnWallTimerService>();
        }

        public void Execute()
        {
            StepPlayersShootCooldown(_deltaTime);
            _startMatchWallController.StepTimer(_deltaTime);
            _headLessQuitterController.StepTimer(_deltaTime);
            _lockOnWallTimerService.StepTimers(_deltaTime);
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