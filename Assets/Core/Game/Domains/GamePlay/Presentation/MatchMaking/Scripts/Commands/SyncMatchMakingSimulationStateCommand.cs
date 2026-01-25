using Core.Game.Domains.GamePlay.Presentation.Features.Bullets.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.MatchMaking.Features.Bullets;
using Core.Game.Domains.GamePlay.Presentation.MatchMaking.Features.Environment.Walls.Scripts.Mvcs;
using Core.Game.Domains.GamePlay.Presentation.MatchMaking.Scripts.DataService;
using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels.MatchMaking;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Presentation.MatchMaking.Scripts.Commands
{
    public class SyncMatchMakingSimulationStateCommand : BaseCommand, ICommandVoid
    {
        private const int MATCH_MAKING_ENVIRONMENT_INDEX = 99;
        
        private MatchMakingSimulationStateS2C _simulationState;
        private IMatchMakingDataService _matchDataService;
        private IMatchMakingBulletControllers _bulletControllers;
        private IMatchMakingEnvironmentWallsControllers _environmentWallsControllers;
        private SharedGamePlayConfig _sharedGamePlayConfig;
        private AddMatchMakingPlayerCommand _addMatchMakingPlayerCommand;
        private ICommandFactory _commandFactory;

        public SyncMatchMakingSimulationStateCommand SetSimulationState(MatchMakingSimulationStateS2C simulationState)
        {
            _simulationState = simulationState;
            return this;
        }
        
        public override void ResolveDependencies()
        {
            _matchDataService = _diContainer.Resolve<IMatchMakingDataService>();
            _bulletControllers = _diContainer.Resolve<IMatchMakingBulletControllers>();
            _environmentWallsControllers = _diContainer.Resolve<IMatchMakingEnvironmentWallsControllers>();
            _sharedGamePlayConfig = _diContainer.Resolve<SharedGamePlayConfig>();
            _commandFactory = _diContainer.Resolve<ICommandFactory>();
            _addMatchMakingPlayerCommand = _commandFactory.CreateCommandVoid<AddMatchMakingPlayerCommand>();
        }

        public void Execute()
        {
            CreatePlayers();
            CreateBullets();
            CreateWalls();
        }

        private void CreatePlayers()
        {
            foreach (var playerState in _simulationState.Players.AsSpan())
            {
                _addMatchMakingPlayerCommand.SetPlayerState(playerState).Execute();
            }
        }

        private void CreateBullets()
        {
            foreach (var bulletState in _simulationState.Bullets.AsSpan())
            {
                _matchDataService.AddBullet(bulletState.Id, bulletState.BelongToPlayerId, bulletState.Position, bulletState.Radius);
                var bulletColor = _matchDataService.GetPlayer(bulletState.BelongToPlayerId).Spaceship.Color;
                _bulletControllers.CreateBullet(bulletState.Id, bulletState.Radius, bulletState.Position, bulletColor);
            }
        }

        private void CreateWalls()
        {
            var walls = _sharedGamePlayConfig.Environment.GetEnvironmentLayout(MATCH_MAKING_ENVIRONMENT_INDEX).GetWalls();
            foreach (var wall in walls)
            {
                var wallModel = _matchDataService.AddWall(wall);
                _environmentWallsControllers.CreateWall(wallModel.Id);
            }
        }
    }
}