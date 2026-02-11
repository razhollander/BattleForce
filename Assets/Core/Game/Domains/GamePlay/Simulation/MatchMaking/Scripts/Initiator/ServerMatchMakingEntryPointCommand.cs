using Core.Game.Domains.GamePlay.Simulation.MatchMaking.Scripts.NetworkManager.TickHandlers.PacketsObservers;
using Core.Game.Domains.GamePlay.Simulation.MatchMaking.Scripts.StartMatchWall;
using Core.Game.Domains.GamePlay.Simulation.MatchMaking.Scripts.TickHandlers.PacketObservers;
using Core.Game.Domains.GamePlay.Shared.Scripts.Configs;
using Core.Game.Domains.GamePlay.Simulation.MatchMaking.Scripts.TeamFloor;
using Core.Game.Domains.GamePlay.Simulation.Scripts.MatchMakingModel.MatchMakingModel;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Physics;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Simulation.MatchMaking.Scripts.Initiator
{
    public class ServerMatchMakingEntryPointCommand: BaseCommand, ICommandVoid
    {
        private IPlayerJoinPacketsHandler _playerJoinPacketsHandler;
        private ITickProcessor _tickProcessor;
        private IPlayerInputsPacketsHandler _playerInputsPacketsHandler;
        private IPhysicsSimulator _physicsSimulator;
        private IMatchMakingDataService _matchMakingDataService;
        private SharedGamePlayConfig _sharedGamePlayConfig;
        private IStartMatchWallController _startMatchWallController;
        private ITeamFloorDataService _teamFloorDataService;

        public override void ResolveDependencies()
        {
            _playerJoinPacketsHandler = _diContainer.Resolve<IPlayerJoinPacketsHandler>();
            _tickProcessor = _diContainer.Resolve<ITickProcessor>();
            _playerInputsPacketsHandler = _diContainer.Resolve<IPlayerInputsPacketsHandler>();
            _physicsSimulator = _diContainer.Resolve<IPhysicsSimulator>();
            _matchMakingDataService = _diContainer.Resolve<IMatchMakingDataService>();
            _sharedGamePlayConfig = _diContainer.Resolve<SharedGamePlayConfig>();
            _startMatchWallController = _diContainer.Resolve<IStartMatchWallController>();
            _teamFloorDataService = _diContainer.Resolve<ITeamFloorDataService>();
        }

        public void Execute()
        {
            _matchMakingDataService.InitEntryPoint();
            _playerInputsPacketsHandler.InitEntryPoint();
            _playerJoinPacketsHandler.InitEntryPoint();
            
            CreateWalls();
            CreateTeamFloors();
            CreateStartMatchWall();
            
            _tickProcessor.InitEntryPoint();
        }

        private void CreateStartMatchWall()
        {
            _startMatchWallController.Initialize(_sharedGamePlayConfig.MatchMakingEnvironment.StartMatchWallRadius);
        }

        private void CreateTeamFloors()
        {
            var walls = DonutQuadrantWalls.GenerateQuadrantWallPerTeam(_sharedGamePlayConfig.MatchMakingEnvironment.TeamFloorsRadius, _sharedGamePlayConfig.MatchMakingEnvironment.TeamFloorsPrecision);
            ushort teamFloorId = PhysicsSimulator.MIN_BOX2D_ID;
            foreach (var kvp in walls)
            {
                var teamId = kvp.Key;
                var wallConfigs = kvp.Value;
                foreach (var wallConfig in wallConfigs)
                {
                    _teamFloorDataService.FloorIdToTeamId.Add(teamFloorId, teamId);
                    _physicsSimulator.AddTeamFloor(teamFloorId, wallConfig.Points);
                    teamFloorId++;
                }
            }
        }

        private void CreateWalls()
        {
            var wallConfigs = _matchMakingDataService.Environment.WallConfigs;

            foreach (var wallConfig in wallConfigs)
            {
                var wallId = wallConfig.Id;
                var wallPoints = wallConfig.Points;
                _physicsSimulator.AddWall(wallId, wallPoints);
            }
        }
    }
}