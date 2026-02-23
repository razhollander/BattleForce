using Core.Game.Domains.GamePlay.Presentation.Match.Features.Bullets.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.Environment.LavaWalls.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.Environment.RotatingWheels.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.Environment.Springs.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.Environment.TeleportGate.Scripts.Mvcs.EnvironmentTeleportGate;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.Environment.Walls.Scripts.Mvcs;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.Player.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.PowerUps.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.TalentCards.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.UI.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.UI.Scripts.TeamsBoard;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.DataService;
using Core.Game.Domains.GamePlay.Presentation.Scripts.ScriptableObjects;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Scripts.Extensions;
using CoreDomain.Scripts.Mvc.WorldCamera;
using CoreDomain.Scripts.Services.CommandFactory;
using CoreDomain.Scripts.Services.Logger.Base;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands.NetEvents
{
    public class SyncMatchSimulationStateCommand : BaseCommand, ICommandVoid
    {
        private MatchSimulationStateS2C _simulationState;
        private IMatchDataService _matchDataService;
        private IMatchBulletControllers _bulletControllers;
        private IMatchEnvironmentWallsControllers _environmentWallsControllers;
        private IEnvironmentSpringControllers _environmentSpringControllers;
        private IMatchEnvironmentRotatingWheelControllers _rotatingWheelControllers;
        private ITalentCardControllers _talentCardControllers;
        private SharedGamePlayConfig _sharedGamePlayConfig;
        private IEnvironmentLavaWallsControllers _environmentLavaWallsControllers;
        private IPowerUpBallControllers _powerUpBallControllers;
        private AddMatchPlayerCommand _addMatchPlayerCommand;
        private ICommandFactory _commandFactory;
        private PresentationGamePlayConfig _gameplayConfig;
        private IMatchPlayerControllers _playerControllers;
        private IMatchPlayerUIControllers _playerUIControllers;
        private IWorldCameraController _worldCameraController;
        private ITeamsBoardUIController _teamsBoardUIController;
        private IEnvironmentTeleportGateControllers _teleportGateControllers;

        public SyncMatchSimulationStateCommand SetSimulationState(MatchSimulationStateS2C simulationState)
        {
            _simulationState = simulationState;
            return this;
        }
        
        public override void ResolveDependencies()
        {
            _matchDataService = _diContainer.Resolve<IMatchDataService>();
            _bulletControllers = _diContainer.Resolve<IMatchBulletControllers>();
            _environmentWallsControllers = _diContainer.Resolve<IMatchEnvironmentWallsControllers>();
            _environmentSpringControllers = _diContainer.Resolve<IEnvironmentSpringControllers>();
            _rotatingWheelControllers = _diContainer.Resolve<IMatchEnvironmentRotatingWheelControllers>();
            _environmentLavaWallsControllers = _diContainer.Resolve<IEnvironmentLavaWallsControllers>();
            _talentCardControllers = _diContainer.Resolve<ITalentCardControllers>();
            _powerUpBallControllers = _diContainer.Resolve<IPowerUpBallControllers>();
            _sharedGamePlayConfig = _diContainer.Resolve<SharedGamePlayConfig>();
            _commandFactory = _diContainer.Resolve<ICommandFactory>();
            _addMatchPlayerCommand = _commandFactory.CreateCommandVoid<AddMatchPlayerCommand>();
            _gameplayConfig =_diContainer.Resolve<PresentationGamePlayConfig>();
            _playerControllers = _diContainer.Resolve<IMatchPlayerControllers>();
            _playerUIControllers = _diContainer.Resolve<IMatchPlayerUIControllers>();
            _worldCameraController = _diContainer.Resolve<IWorldCameraController>();
            _teamsBoardUIController = _diContainer.Resolve<ITeamsBoardUIController>();
            _teleportGateControllers = _diContainer.Resolve<IEnvironmentTeleportGateControllers>();
        }

        public void Execute()
        {
            DestroyAll();
            CreateAll();
        }

        private void DestroyAll()
        {
            _worldCameraController.ClearTargets();
            _matchDataService.ClearAll();
            _bulletControllers.DestroyAll();
            _environmentWallsControllers.DestroyAll();
            _environmentSpringControllers.DestroyAll();
            _rotatingWheelControllers.DestroyAll();
            _environmentLavaWallsControllers.DestroyAll();
            _talentCardControllers.DestroyAll();
            _powerUpBallControllers.DestroyAll();
            _playerControllers.DestroyAll();
            _playerUIControllers.DestroyAll();
            _teamsBoardUIController.DestroyAll();
            _teleportGateControllers.DestroyAll();
        }

        private void CreateAll()
        {
            CreatePlayers();
            CreateBullets();
            CreateWalls();
            CreateSprings();
            CreateLavaWalls();
            CreateRotatingWheels();
            CreateTalentCards();
            CreatePowerUpBalls();
            CreateTeamBoards();
            CreateTeleportGates();
        }

        private void CreateTeamBoards()
        {
            foreach (ushort teamId in _matchDataService.TeamIds)
            {
                var teamGems = _simulationState.GemsPerTeamId[teamId];
                var teamBolts = _simulationState.BoltsPerTeam[teamId];
                _matchDataService.SetTeamBolts(teamId, teamBolts);
                _matchDataService.SetTeamGems(teamId, teamGems);
                _teamsBoardUIController.CreateTeamBoard(teamId, teamGems, teamBolts);
            }
        }

        private void CreateSprings()
        {
            var springs = _sharedGamePlayConfig.Environment.GetEnvironmentLayout(_simulationState.EnvironmentLayoutIndex).GetEnvironmentSprings();
            
            foreach (var spring in springs)
            {
                _matchDataService.AddSpring(spring.Id, spring.Position.ToUnityVector2(), spring.DirectionAngle);
                _environmentSpringControllers.CreateSpring(spring.Id);
            }
        }

        private void CreateTeleportGates()
        {
            var teleportGatePairConfigs = _sharedGamePlayConfig.Environment.GetEnvironmentLayout(_simulationState.EnvironmentLayoutIndex).GetTeleportGates();
            if (teleportGatePairConfigs.IsNullOrEmpty())
            {
                return;
            }

            foreach (var teleportGatePairConfig in teleportGatePairConfigs)
            {
                var gateSize = _sharedGamePlayConfig.EnvironmentTeleport.Size;
                _matchDataService.AddTeleportPair(teleportGatePairConfig.Id, teleportGatePairConfig.GateAId, teleportGatePairConfig.GateA.Position, teleportGatePairConfig.GateA.NormalRotation, teleportGatePairConfig.GateBId, teleportGatePairConfig.GateB.Position, teleportGatePairConfig.GateB.NormalRotation, gateSize.ToNumericsVector2());
                _teleportGateControllers.CreateGatePair(teleportGatePairConfig.Id);
            }
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
                var bulletColor = _gameplayConfig.ColorPerTeamId[_matchDataService.GetPlayer(bulletState.BelongToPlayerId).TeamId];
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

        private void CreateRotatingWheels()
        {
            var wheels = _sharedGamePlayConfig.Environment.GetEnvironmentLayout(_simulationState.EnvironmentLayoutIndex).GetRotatingWheels();
            if (wheels == null) return;

            foreach (var wheelConfig in wheels)
            {
                var wheelModel = _matchDataService.AddEnvironmentRotatingWheel(wheelConfig);
                var wheelCenter = wheelModel.CenterPosition;
                var rotationSpeed = wheelModel.RotationSpeed;

                // Create initial state (tick 0)
                // Register children with INITIAL WORLD positions/rotations.
                // View creation uses these world positions, parenting handles the rest.

                if (wheelConfig.Walls != null)
                {
                    foreach (var wallConfig in wheelConfig.Walls)
                    {
                        // Walls in config have points. Assuming points are relative to Wheel Center.
                        // We register them as is? AddWall registers points.
                        // If EnvironmentWallView uses points relative to its transform...
                        // And we parent it to Wheel...
                        // We want the Wall View to be at WheelPos.
                        // So we register a WallModel (which creates ID).
                        // Note: We don't transform points here because View parenting handles it?
                        // BUT AddWall returns a Model.
                        // MatchEnvironmentWallsControllers uses AddWall(config).
                        // Wait, AddWall(config) uses config points.
                        // If config points are relative to wheel, and we parent to wheel, it works.
                        // So we just register.
                        _matchDataService.AddWall(wallConfig);
                    }
                }

                if (wheelConfig.LavaWalls != null)
                {
                    foreach (var wallConfig in wheelConfig.LavaWalls)
                    {
                        _matchDataService.AddLavalWall(wallConfig);
                    }
                }

                if (wheelConfig.Springs != null)
                {
                    foreach (var springConfig in wheelConfig.Springs)
                    {
                        // Calculate initial world position/rotation
                        Core.Game.Domains.GamePlay.Shared.Scripts.Utils.EnvironmentRotatingWheelUtils.CalculateChildTransform(
                            0, rotationSpeed, 0, wheelConfig.CenterPosition, springConfig.Position, springConfig.DirectionAngle,
                            out var worldPos, out var worldRot
                        );

                        _matchDataService.AddSpring(springConfig.Id, worldPos.ToUnityVector2(), worldRot);
                    }
                }
            }
            _rotatingWheelControllers.CreateRotatingWheels();
        }
    }
}