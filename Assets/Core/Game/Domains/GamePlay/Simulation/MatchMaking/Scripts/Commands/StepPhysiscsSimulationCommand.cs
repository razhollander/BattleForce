using System.Numerics;
using Core.Game.Domains.GamePlay.Simulation.Scripts.MatchMakingModel.MatchMakingModel;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Physics;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Services.PlayersForcesService;
using Core.Scripts.Network;
using Core.Scripts.Utils.CustomCollections;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Simulation.MatchMaking.Scripts.Commands
{
    public class StepPhysiscsSimulationCommand : BaseCommand, ICommandVoid
    {
        private IMatchMakingDataService _matchDataService;
        private IPhysicsSimulator _physicsSimulator;
        private NetworkConfig _networkConfig;
        private IPlayersVelocityService _iPlayersVelocityService;
        private CapacityDict<ushort, Vector2> _cachedVecolcityPerPlayer;
        private float _deltaTime;

        public StepPhysiscsSimulationCommand SetDeltaTime(float deltaTime)
        {
            _deltaTime = deltaTime;
            return this;
        }
        
        public override void ResolveDependencies()
        {
            _matchDataService = _diContainer.Resolve<IMatchMakingDataService>();
            _physicsSimulator = _diContainer.Resolve<IPhysicsSimulator>();
            _networkConfig = _diContainer.Resolve<NetworkConfig>();
            _cachedVecolcityPerPlayer = new CapacityDict<ushort, Vector2>(_networkConfig.MaxCap.ConcurrentPlayers);
        }

        public void Execute()
        {
            StepPhysics(_deltaTime);
        }

        private void StepPhysics(float stepDeltaTime)
        {
            ApplyMatchModelToPhysicsSimulation();
            _physicsSimulator.Step(stepDeltaTime, _networkConfig.PhysicsVelocityIterations, _networkConfig.PositionIterations);
            ApplyPhysicsSimulationToMatchModel();
        }

        private void ApplyMatchModelToPhysicsSimulation()
        {
            foreach (var playerSate in _matchDataService.SimulationState.Players.AsSpan())
            {
                var playerVelocity = _iPlayersVelocityService.CalculatePlayerVelocity(playerSate.Id);
                _cachedVecolcityPerPlayer[playerSate.Id] = playerVelocity;
            }

            _physicsSimulator.CopyDataToSimulation(_matchDataService.SimulationState, _cachedVecolcityPerPlayer);
        }

        private void ApplyPhysicsSimulationToMatchModel()
        {
            for (int i = 0; i < _matchDataService.SimulationState.Players.Count; i++)
            {
                var playerState = _matchDataService.SimulationState.Players.GetByIndex(i);
                playerState.Spaceship.Transform.Position = _physicsSimulator.GetPlayer(playerState.Id).Position;
            }

            for (int i = 0; i < _matchDataService.SimulationState.Bullets.Count; i++)
            {
                ref var bulletState = ref _matchDataService.SimulationState.Bullets.GetByIndex(i);
                bulletState.Position = _physicsSimulator.GetBullet(bulletState.Id).Position;
            }
        }
    }
}