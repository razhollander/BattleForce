using System.Numerics;
using Core.Game.Domains.GamePlay.Shared.NetworkManager;
using Core.Game.Domains.GamePlay.Simulation.NetworkManager;
using Core.Game.Domains.GamePlay.Simulation.NetworkManager.PacketsHandlers;
using Core.Game.Domains.GamePlay.Simulation.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager.Configurations;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager.TickHandlers.PacketsObservers.PacketsHandlers;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Physics;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.ContextInstaller
{
    public class ServerEntryPointCommand : BaseCommand, ICommandVoid
    {
        private IServerNetworkManager _serverNetworkManager;
        private IPlayerJoinPacketsHandler _playerJoinPacketsHandler;
        private ITickProcessor _tickProcessor;
        private IPlayerInputsPacketsHandler _playerInputsPacketsHandler;
        private IPhysicsSimulator _physicsSimulator;
        private SimulationGamePlayConfig _simulationGamePlayConfig;
        private IMatchDataService _matchDataService;

        public override void ResolveDependencies()
        {
            _serverNetworkManager = _diContainer.Resolve<IServerNetworkManager>();
            _playerJoinPacketsHandler = _diContainer.Resolve<IPlayerJoinPacketsHandler>();
            _tickProcessor = _diContainer.Resolve<ITickProcessor>();
            _playerInputsPacketsHandler = _diContainer.Resolve<IPlayerInputsPacketsHandler>();
            _physicsSimulator = _diContainer.Resolve<IPhysicsSimulator>();
            _simulationGamePlayConfig = _diContainer.Resolve<SimulationGamePlayConfig>();
            _matchDataService = _diContainer.Resolve<IMatchDataService>();
        }

        public void Execute()
        {
            _serverNetworkManager.InitEntryPoint();
            _playerInputsPacketsHandler.InitEntryPoint();
            _playerJoinPacketsHandler.InitEntryPoint();
            _tickProcessor.InitEntryPoint();
            _physicsSimulator.InitEntryPoint();
            
            CreateWalls();
        }

        private void CreateWalls()
        {
            var wallConfigs = _simulationGamePlayConfig.Environment.GetWalls();

            foreach (var wallConfig in wallConfigs)
            {
                var wallId = wallConfig.Id;
                var wallPoints = wallConfig.Points;
                _matchDataService.SimulationState.AddWall(wallId, wallPoints);
                _physicsSimulator.AddWall(wallId, wallPoints);
            }
        }
    }
}