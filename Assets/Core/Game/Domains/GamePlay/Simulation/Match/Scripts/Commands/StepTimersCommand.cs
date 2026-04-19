using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.PlayersInLavaTracker;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.PowerUpsSpawner;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Controllers;
using CoreDomain.Scripts.Services.CommandFactory;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Physics;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;
using Core.Scripts.Network;
using Core.Scripts.Utils;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Commands
{
    public class StepTimersCommand : BaseCommand, ICommandVoid
    {
        private IMatchDataService _matchDataService;
        private IPowerUpsSpawnerService _powerUpsSpawnerService;
        private IPlayersInLavaTrackerService _playersInLavaTrackerService;
        private IHeadLessQuitterController _headLessQuitterController;
        private IPreparationPhaseTimerService _preparationPhaseTimerService;
        private INetEventsDataService _netEventsDataService;
        private IPhysicsSimulator _physicsSimulator;
        private SimulationGamePlayConfig _gamePlayConfig;
        private NetworkConfig _networkConfig;
        
        private float _deltaTime;
        private int _processedTick;
   
        public StepTimersCommand SetStepDeltaTime(float deltaTime, int processedTick)
        {
            _deltaTime = deltaTime;
            _processedTick = processedTick;
            return this;
        }

        public override void ResolveDependencies()
        {
            _matchDataService = _diContainer.Resolve<IMatchDataService>();
            _powerUpsSpawnerService = _diContainer.Resolve<IPowerUpsSpawnerService>();
            _playersInLavaTrackerService = _diContainer.Resolve<IPlayersInLavaTrackerService>();
            _headLessQuitterController = _diContainer.Resolve<IHeadLessQuitterController>();
            _preparationPhaseTimerService = _diContainer.Resolve<IPreparationPhaseTimerService>();
            _netEventsDataService = _diContainer.Resolve<INetEventsDataService>();
            _physicsSimulator = _diContainer.Resolve<IPhysicsSimulator>();
            _gamePlayConfig = _diContainer.Resolve<SimulationGamePlayConfig>();
            _networkConfig = _diContainer.Resolve<NetworkConfig>();
        }

        public void Execute()
        {
            StepPlayersShootCooldown(_deltaTime);
            _powerUpsSpawnerService.StepTimer(_deltaTime);
            _playersInLavaTrackerService.StepTimePassedSinceLastDamageTaken(_deltaTime);
            _headLessQuitterController.StepTimer(_deltaTime);
            StepPreperationPhaseTimer(_deltaTime);
            StepChickenEggs();
        }

        private void StepChickenEggs()
        {
            var eggs = _matchDataService.SimulationState.ChickenEggs;
            var config = _gamePlayConfig.Talents.ChickenTalentConfig;
            var ticksToDestroy = TickUtils.GetTicksPassedDuration(config.DestroyDelayInSeconds, _networkConfig.DeltaTime);

            for (int i = eggs.Count - 1; i >= 0; i--)
            {
                var egg = eggs[i];
                if (egg.IsBroken && _processedTick > egg.BrokenTick + ticksToDestroy)
                {
                    _netEventsDataService.AddDestroyChickenEggNetEventS2C(_processedTick, egg.Id);
                    _physicsSimulator.RemoveChickenEgg(egg.Id);
                    _matchDataService.SimulationState.RemoveChickenEggById(egg.Id);
                }
            }
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