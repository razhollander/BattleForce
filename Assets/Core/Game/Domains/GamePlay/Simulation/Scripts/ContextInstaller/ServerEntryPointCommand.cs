using Core.Game.Domains.GamePlay.Shared.NetworkManager;
using Core.Game.Domains.GamePlay.Simulation.NetworkManager;
using Core.Game.Domains.GamePlay.Simulation.NetworkManager.PacketsHandlers;
using Core.Game.Domains.GamePlay.Simulation.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager.Configurations;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager.TickHandlers.PacketsObservers.PacketsHandlers;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Physics;
using Core.Scripts.Extensions;
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
        private SharedGamePlayConfig _sharedGamePlayConfig;
        private ILavaManager _lavaManager;

        public override void ResolveDependencies()
        {
            _serverNetworkManager = _diContainer.Resolve<IServerNetworkManager>();
            _playerJoinPacketsHandler = _diContainer.Resolve<IPlayerJoinPacketsHandler>();
            _tickProcessor = _diContainer.Resolve<ITickProcessor>();
            _playerInputsPacketsHandler = _diContainer.Resolve<IPlayerInputsPacketsHandler>();
            _physicsSimulator = _diContainer.Resolve<IPhysicsSimulator>();
            _simulationGamePlayConfig = _diContainer.Resolve<SimulationGamePlayConfig>();
            _sharedGamePlayConfig = _diContainer.Resolve<SharedGamePlayConfig>();
            _matchDataService = _diContainer.Resolve<IMatchDataService>();
            _lavaManager = _diContainer.Resolve<ILavaManager>();
        }

        public void Execute()
        {
            _serverNetworkManager.InitEntryPoint();
            _playerInputsPacketsHandler.InitEntryPoint();
            _playerJoinPacketsHandler.InitEntryPoint();
            _tickProcessor.InitEntryPoint();
            _physicsSimulator.InitEntryPoint();
            _lavaManager.InitEntryPoint();
            
            CreateWalls();
            CreateLavaWalls();
            CreateTalentCards();
        }

        private void CreateWalls()
        {
            var wallConfigs = _sharedGamePlayConfig.Environment.GetEnvironmentLayout(_matchDataService.SimulationState.EnvironmentLayoutIndex).GetWalls();

            foreach (var wallConfig in wallConfigs)
            {
                var wallId = wallConfig.Id;
                var wallPoints = wallConfig.Points;
                _matchDataService.AddWall(wallId, wallPoints);
                _physicsSimulator.AddWall(wallId, wallPoints);
            }
        }

        private void CreateLavaWalls()
        {
            var wallConfigs = _sharedGamePlayConfig.Environment.GetEnvironmentLayout(_matchDataService.SimulationState.EnvironmentLayoutIndex).GetLavaWalls();
            if (wallConfigs.IsNullOrEmpty())
            {
                return;
            }

            foreach (var wallConfig in wallConfigs)
            {
                var wallId = wallConfig.Id;
                var wallPoints = wallConfig.Points;
                // Lava walls are not part of match state for clients?
                // "Created and implemented like the Environment Walls"
                // MatchDataService.AddWall is used for sync state. If clients need to see them, we should add them.
                // But AddWall adds "Wall" type. We probably need AddLavaWall in MatchDataService or just treat them as walls for visual/sync?
                // The requirements say "Created and implemented like the Environment Walls".
                // If I add them as Walls in MatchDataService, they might be rendered as walls.
                // Clients might need to know they are Lava.
                // However, I will assume for now only Physics needs to know it's Lava.
                // If visuals are needed, we need a new NetEvent or Sync data.
                // The task says "Add a new type of wall called Lava".
                // I'll skip MatchDataService.AddWall(Lava) unless I modify MatchDataService to support Lava types.
                // But let's check MatchEnvironmentWallModel.
                _physicsSimulator.AddLava(wallId, wallPoints);
            }
        }

        private void CreateTalentCards()
        {
            var talentCards = _sharedGamePlayConfig.Environment.GetEnvironmentLayout(_matchDataService.SimulationState.EnvironmentLayoutIndex).GetTalentCards();
            if (talentCards.IsNullOrEmpty())
            {
                return;
            }
            
            foreach (var talentCard in talentCards)
            {
                var talentCardPosition = talentCard.Position;
                var talentCardId = talentCard.Id;
                _matchDataService.AddTalentCard(talentCardId, talentCardPosition, talentCard.TalentType, _simulationGamePlayConfig.Talents.TalentCardHealth);
                _physicsSimulator.AddTalentCard(talentCardId, talentCardPosition, _simulationGamePlayConfig.Talents.TalentCardWidth, _simulationGamePlayConfig.Talents.TalentCardHeight);
            }
        }
    }
}