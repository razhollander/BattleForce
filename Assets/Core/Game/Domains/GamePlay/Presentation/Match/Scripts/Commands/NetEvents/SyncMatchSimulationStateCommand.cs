using System.Collections.Generic;
using System.Numerics;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.Bullets.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.DashPulse.Scripts.Effect;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.Environment.FieldBarriers.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.Environment.LavaWalls.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.Environment.Springs.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.Environment.TeleportGate.Scripts.Mvcs.EnvironmentTeleportGate;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.Environment.Walls.Scripts.Mvcs;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.KOProjectiles.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.Player.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.PowerUps.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.SwapFields.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.TalentCards.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.UI.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.UI.Scripts.TeamsBoard;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.DataService;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.StageCancellationToken;
using Core.Game.Domains.GamePlay.Presentation.Scripts.Network.PacketsHandlers;
using Core.Game.Domains.GamePlay.Presentation.Scripts.ScriptableObjects;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Shared.Scripts.Utils;
using Core.Scripts.Extensions;
using Core.Scripts.Network;
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
        private IFullTickPacketsHandler _fullTickPacketsHandler;
        private NetworkConfig _networkConfig;
        private IEnvironmentFieldBarrierControllers _environmentFieldBarrierControllers;
        private ISwapFieldControllers _swapFieldControllers;
        private IKOProjectilesControllers _kOProjectilesControllers;
        private IStageCancellationTokenProvider _stageCancellationTokenProvider;

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
            _fullTickPacketsHandler = _diContainer.Resolve<IFullTickPacketsHandler>();
            _networkConfig = _diContainer.Resolve<NetworkConfig>();
            _environmentFieldBarrierControllers = _diContainer.Resolve<IEnvironmentFieldBarrierControllers>();
            _swapFieldControllers = _diContainer.Resolve<ISwapFieldControllers>();
            _kOProjectilesControllers = _diContainer.Resolve<IKOProjectilesControllers>();
            _stageCancellationTokenProvider = _diContainer.Resolve<IStageCancellationTokenProvider>();
        }

        public void Execute()
        {
            _matchDataService.StartPhaseInitialTick = _simulationState.StartPhaseInitialTick;
            _matchDataService.IsInPreparationPhase = _simulationState.IsInPreparationPhase;
            _stageCancellationTokenProvider.CancelAndRegenarateStageToken();
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
            _environmentLavaWallsControllers.DestroyAll();
            _talentCardControllers.DestroyAll();
            _powerUpBallControllers.DestroyAll();
            _playerControllers.DestroyAll();
            _playerUIControllers.DestroyAll();
            _teamsBoardUIController.DestroyAll();
            _teleportGateControllers.DestroyAll();
            _environmentFieldBarrierControllers.DestroyAll();
            _swapFieldControllers.DestroyAll();
            _kOProjectilesControllers.DestroyAll();
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
            CreateFieldBarriers();
        }

        private void CreateFieldBarriers()
        {
            var barrierConfigs = _sharedGamePlayConfig.Environment.GetEnvironmentLayout(_simulationState.EnvironmentLayoutId).GetFieldBarriers();
            if (barrierConfigs.IsNullOrEmpty())
            {
                return;
            }

            var teamIds = new List<ushort>(_matchDataService.TeamIds);
            teamIds.Sort();
            ushort barrierIndex = 0;
            foreach (var teamId in teamIds)
            {
                var barrierConfig = barrierConfigs[barrierIndex];
                _matchDataService.AddFieldBarrier(barrierIndex, teamId, barrierConfig.Position, barrierConfig.Size, barrierConfig.Shape);
                _environmentFieldBarrierControllers.CreateFieldBarrier(barrierIndex);
                barrierIndex++;
            }
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
            var springs = _sharedGamePlayConfig.Environment.GetEnvironmentLayout(_simulationState.EnvironmentLayoutId).GetEnvironmentSprings();
            if (springs.IsNullOrEmpty())
            {
                return;
            }
            
            foreach (var spring in springs)
            {
                _matchDataService.AddSpring(spring.Id, Vector2.Zero, spring.Position, 0, spring.RotationAngle);
                _environmentSpringControllers.CreateSpring(spring.Id);
            }
        }

        private void CreateTeleportGates()
        {
            var teleportGatePairConfigs = _sharedGamePlayConfig.Environment.GetEnvironmentLayout(_simulationState.EnvironmentLayoutId).GetTeleportGates();
            if (teleportGatePairConfigs.IsNullOrEmpty())
            {
                return;
            }

            foreach (var teleportGatePairConfig in teleportGatePairConfigs)
            {
                var gateSize = _sharedGamePlayConfig.EnvironmentTeleport.Size;
                _matchDataService.AddTeleportPair(teleportGatePairConfig.Id, teleportGatePairConfig.GateAId, teleportGatePairConfig.GateA.Position, teleportGatePairConfig.GateA.NormalRotation, teleportGatePairConfig.GateBId, teleportGatePairConfig.GateB.Position, teleportGatePairConfig.GateB.NormalRotation, teleportGatePairConfig.GateA.Position, teleportGatePairConfig.GateA.NormalRotation, teleportGatePairConfig.GateB.Position, teleportGatePairConfig.GateB.NormalRotation, gateSize.ToNumericsVector2());
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
                _addMatchPlayerCommand.SetPlayerState(playerState).SetCurrentServerTick(_fullTickPacketsHandler.LastProcessedTickFromServer).Execute();
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
            var walls = _sharedGamePlayConfig.Environment.GetEnvironmentLayout(_simulationState.EnvironmentLayoutId).GetWalls();
            if (walls.IsNullOrEmpty())
            {
                return;
            }
            
            foreach (var wall in walls)
            {
                var wallModel = _matchDataService.AddWall(wall.Id, wall.Points, Vector2.Zero, wall.Position, 0);
                _environmentWallsControllers.CreateWall(wallModel.Id);
            }
        }
        
        private void CreateLavaWalls()
        {
            var lavaWalls = _sharedGamePlayConfig.Environment.GetEnvironmentLayout(_simulationState.EnvironmentLayoutId).GetLavaWalls();
            if (lavaWalls.IsNullOrEmpty())
            {
                return;
            }
            
            foreach (var lavaWall in lavaWalls)
            {
                var lavaWallModel = _matchDataService.AddLavalWall(lavaWall.Id, lavaWall.Points, Vector2.Zero, lavaWall.Position, 0);
                _environmentLavaWallsControllers.CreateLavaWall(lavaWallModel.Id);
            }
        }

        private void CreateRotatingWheels()
        {
            var wheels = _sharedGamePlayConfig.Environment.GetEnvironmentLayout(_simulationState.EnvironmentLayoutId).GetRotatingWheels();
            if (wheels.IsNullOrEmpty())
            {
                return;
            }
            var lastProcessedTickFromServer = _fullTickPacketsHandler.LastProcessedTickFromServer;
            var calculationTick = _matchDataService.IsInPreparationPhase ? 0 : lastProcessedTickFromServer - _matchDataService.StartPhaseInitialTick;
            var deltaTime = _networkConfig.DeltaTime;

            foreach (var wheelConfig in wheels)
            {
                var wheelModel = _matchDataService.AddEnvironmentRotatingWheel(wheelConfig);
                var rotationSpeed = wheelModel.RotationSpeed;
                
                if (wheelConfig.Walls != null)
                {
                    foreach (var wallConfig in wheelConfig.Walls)
                    {
                        EnvironmentRotatingWheelUtils.CalculateChildTransform(
                            calculationTick, rotationSpeed, deltaTime, wheelConfig.CenterPosition, wallConfig.Position, 0,
                            out var worldPosition, out var worldRotation
                        );
                        
                        _matchDataService.AddWall(wallConfig.Id, wallConfig.Points, wallConfig.Position, worldPosition, worldRotation);
                        _environmentWallsControllers.CreateWall(wallConfig.Id);
                    }
                }

                if (wheelConfig.LavaWalls != null)
                {
                    foreach (var lavaWallConfig in wheelConfig.LavaWalls)
                    {
                        EnvironmentRotatingWheelUtils.CalculateChildTransform(
                            calculationTick, rotationSpeed, deltaTime, wheelConfig.CenterPosition, lavaWallConfig.Position, 0,
                            out var worldPosition, out var worldRotation
                        );
                        
                        _matchDataService.AddLavalWall(lavaWallConfig.Id, lavaWallConfig.Points, lavaWallConfig.Position, worldPosition, worldRotation);
                        _environmentLavaWallsControllers.CreateLavaWall(lavaWallConfig.Id);
                    }
                }

                if (wheelConfig.Springs != null)
                {
                    foreach (var springConfig in wheelConfig.Springs)
                    {
                        EnvironmentRotatingWheelUtils.CalculateChildTransform(
                            calculationTick, rotationSpeed, deltaTime, wheelConfig.CenterPosition, springConfig.Position, springConfig.RotationAngle,
                            out var worldPosition, out var worldRotation
                        );

                        _matchDataService.AddSpring(springConfig.Id, springConfig.Position, worldPosition, springConfig.RotationAngle, worldRotation);
                        _environmentSpringControllers.CreateSpring(springConfig.Id);
                    }
                }

                if (wheelConfig.TeleportGatePairs != null)
                {
                    foreach (var teleportPairConfig in wheelConfig.TeleportGatePairs)
                    {
                        var gateSize = _sharedGamePlayConfig.EnvironmentTeleport.Size;

                        EnvironmentRotatingWheelUtils.CalculateChildTransform(
                            calculationTick, rotationSpeed, deltaTime, wheelConfig.CenterPosition, teleportPairConfig.GateA.Position, teleportPairConfig.GateA.NormalRotation,
                            out var worldPositionA, out var worldRotationA
                        );

                        EnvironmentRotatingWheelUtils.CalculateChildTransform(
                            calculationTick, rotationSpeed, deltaTime, wheelConfig.CenterPosition, teleportPairConfig.GateB.Position, teleportPairConfig.GateB.NormalRotation,
                            out var worldPositionB, out var worldRotationB
                        );

                        _matchDataService.AddTeleportPair(teleportPairConfig.Id, teleportPairConfig.GateAId, teleportPairConfig.GateA.Position, teleportPairConfig.GateA.NormalRotation, teleportPairConfig.GateBId, teleportPairConfig.GateB.Position, teleportPairConfig.GateB.NormalRotation, worldPositionA, worldRotationA, worldPositionB, worldRotationB, gateSize.ToNumericsVector2());
                        _teleportGateControllers.CreateGatePair(teleportPairConfig.Id);
                    }
                }
            }
        }
    }
}