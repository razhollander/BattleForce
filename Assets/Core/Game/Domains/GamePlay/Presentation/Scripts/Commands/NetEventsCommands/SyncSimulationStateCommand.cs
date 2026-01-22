using Core.Game.Domains.GamePlay.Presentation.Features.Bullets.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Features.Environment.Walls;
using Core.Game.Domains.GamePlay.Presentation.Features.Player.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Features.Player.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Features.PowerUps.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Features.PowerUps.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Features.TalentCards.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Features.UI.Match.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Scripts.Extensions;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Presentation.Scripts.Commands.NetEventsCommands
{
    public class SyncSimulationStateCommand : BaseCommand, ICommandVoid
    {
        private SimulationStateS2C _simulationState;
        private IMatchDataService _matchDataService;
        private IPlayerControllers _playerControllers;
        private IBulletControllers _bulletControllers;
        private IEnvironmentWallsControllers _environmentWallsControllers;
        private ITalentCardControllers _talentCardControllers;
        private SharedGamePlayConfig _sharedGamePlayConfig;
        private IEnvironmentLavaWallsControllers _environmentLavaWallsControllers;
        private IPowerUpBallControllers _powerUpBallControllers;
        private IMatchPlayerUIControllers _playerUIControllers;
        private AddMatchPlayerCommand _addMatchPlayerCommand;
        private ICommandFactory _commandFactory;

        public SyncSimulationStateCommand SetSimulationState(SimulationStateS2C simulationState)
        {
            _simulationState = simulationState;
            return this;
        }
        
        public override void ResolveDependencies()
        {
            _matchDataService = _diContainer.Resolve<IMatchDataService>();
            _playerControllers = _diContainer.Resolve<IPlayerControllers>();
            _bulletControllers = _diContainer.Resolve<IBulletControllers>();
            _environmentWallsControllers = _diContainer.Resolve<IEnvironmentWallsControllers>();
            _environmentLavaWallsControllers = _diContainer.Resolve<IEnvironmentLavaWallsControllers>();
            _talentCardControllers = _diContainer.Resolve<ITalentCardControllers>();
            _powerUpBallControllers = _diContainer.Resolve<IPowerUpBallControllers>();
            _sharedGamePlayConfig = _diContainer.Resolve<SharedGamePlayConfig>();
            _playerUIControllers = _diContainer.Resolve<IMatchPlayerUIControllers>();
            _commandFactory = _diContainer.Resolve<ICommandFactory>();
            _addMatchPlayerCommand = _commandFactory.CreateCommandVoid<AddMatchPlayerCommand>();
        }

        public void Execute()
        {
            CreatePlayers();
            CreateBullets();
            CreateWalls();
            CreateLavaWalls();
            CreateTalentCards();
            CreatePowerUpBalls();
        }

        private void CreatePowerUpBalls()
        {
            foreach (var powerUpBall in _simulationState.PowerUpBalls.AsSpan())
            {
                var position = powerUpBall.Position.ToUnityVector2();
                _matchDataService.AddPowerUpBall(powerUpBall.Id, position);
                _powerUpBallControllers.CreatePowerUpBall(powerUpBall.Id, position);
            }
        }
        
        private void CreateTalentCards()
        {
            foreach (var talentCard in _simulationState.TalentCards.AsSpan())
            {
                _matchDataService.AddTalentCard(talentCard.Id, talentCard.Position.ToUnityVector2(), talentCard.TalentType, talentCard.Health);
                _talentCardControllers.CreateTalentCard(talentCard.Id);
            }
        }

        private void CreatePlayers()
        {
            foreach (var playerState in _simulationState.Players.AsSpan())
            {
                _addMatchPlayerCommand.SetPlayerState(playerState).Execute();
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
            var walls = _sharedGamePlayConfig.Environment.GetEnvironmentLayout(_simulationState.EnvironmentLayoutIndex).GetWalls();
            foreach (var wall in walls)
            {
                var wallModel = _matchDataService.AddWall(wall);
                _environmentWallsControllers.CreateWall(wallModel.Id);
            }
        }
        
        private void CreateLavaWalls()
        {
            var lavaWalls = _sharedGamePlayConfig.Environment.GetEnvironmentLayout(_simulationState.EnvironmentLayoutIndex).GetLavaWalls();
            foreach (var lavaWall in lavaWalls)
            {
                var lavaWallModel = _matchDataService.AddLavalWall(lavaWall);
                _environmentLavaWallsControllers.CreateLavaWall(lavaWallModel.Id);
            }
        }
    }
}