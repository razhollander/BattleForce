using System.Collections.Generic;
using System.Numerics;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.Bullets.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.ChickenEggs.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.DashPulse.Scripts.Effect;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.Environment.FieldBarriers.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.Environment.LavaWalls.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.Environment.Springs.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.Environment.TeleportGate.Scripts.Mvcs.EnvironmentTeleportGate;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.Environment.Walls.Scripts.Mvcs;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.GrapplingHook.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.KOProjectiles.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.LockOnHeartSights.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.MagneticPullEffect.Scripts;
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
using Core.Scripts.Mvc.WorldCamera;
using Core.Scripts.Network;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands.NetEvents
{
    public class SyncMatchSimulationStateCommand : BaseCommand, ICommandVoid
    {
        private IMatchDataService _matchDataService;
        private IMatchBulletControllers _bulletControllers;
        private IMatchChickenEggsControllers _chickenEggsControllers;
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
        private IGrapplingHookProjectilesControllers _grapplingHookProjectilesControllers;
        private ILockOnTargetEffectController _lockOnTargetEffectController;
        
        private MatchSimulationStateS2C _simulationState;
        private int _stateOccouredOnTick;

        public SyncMatchSimulationStateCommand SetSimulationState(MatchSimulationStateS2C simulationState)
        {
            _simulationState = simulationState;
            return this;
        }
        
        public SyncMatchSimulationStateCommand SetOccuredOnTick(int stateOccouredOnTick)
        {
            _stateOccouredOnTick = stateOccouredOnTick;
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
            _grapplingHookProjectilesControllers = _diContainer.Resolve<IGrapplingHookProjectilesControllers>();
            _chickenEggsControllers = _diContainer.Resolve<IMatchChickenEggsControllers>();
            _lockOnTargetEffectController = _diContainer.Resolve<ILockOnTargetEffectController>();
        }

        public void Execute()
        {
            _matchDataService.StartPhaseInitialTick = _simulationState.StartPhaseInitialTick;
            _matchDataService.IsInPreparationPhase = _simulationState.IsInPreparationPhase;
            _matchDataService.IsInShowoffWinners = _simulationState.IsInShowoffWinners;
            _matchDataService.CurrentStageWinnerTeamId = _simulationState.CurrentStageWinnerTeamId;
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
            _grapplingHookProjectilesControllers.DestroyAll();
            _chickenEggsControllers.DestroyAll();
            _lockOnTargetEffectController.DestroyAll();
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
            CreateSwapField();
            CreateKOPRojectiles();
            CreateGrapplingHookPRojectiles();
            CreateChickenEggs();
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
            var multiplier = _simulationState.MapSizeMultiplier;
            var springs = _sharedGamePlayConfig.Environment.GetEnvironmentLayout(_simulationState.EnvironmentLayoutId).GetEnvironmentSprings();
            if (springs.IsNullOrEmpty())
            {
                return;
            }
            
            foreach (var spring in springs)
            {
                _matchDataService.AddSpring(spring.Id, Vector2.Zero, spring.Position * multiplier, 0, spring.RotationAngle);
                _environmentSpringControllers.CreateSpring(spring.Id);
            }
        }

        private void CreateTeleportGates()
        {
            var multiplier = _simulationState.MapSizeMultiplier;
            var teleportGatePairConfigs = _sharedGamePlayConfig.Environment.GetEnvironmentLayout(_simulationState.EnvironmentLayoutId).GetTeleportGates();
            if (teleportGatePairConfigs.IsNullOrEmpty())
            {
                return;
            }

            foreach (var teleportGatePairConfig in teleportGatePairConfigs)
            {
                var gateSize = _sharedGamePlayConfig.EnvironmentTeleport.Size;
                _matchDataService.AddTeleportPair(teleportGatePairConfig.Id, teleportGatePairConfig.GateAId, teleportGatePairConfig.GateA.Position * multiplier, teleportGatePairConfig.GateA.NormalRotation, teleportGatePairConfig.GateBId, teleportGatePairConfig.GateB.Position * multiplier, teleportGatePairConfig.GateB.NormalRotation, teleportGatePairConfig.GateA.Position * multiplier, teleportGatePairConfig.GateA.NormalRotation, teleportGatePairConfig.GateB.Position * multiplier, teleportGatePairConfig.GateB.NormalRotation, gateSize.ToNumericsVector2());
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
                _matchDataService.AddBullet(bulletState.Id, bulletState.BelongToPlayerId, bulletState.Position, bulletState.Velocity, bulletState.Radius, _stateOccouredOnTick);
                var bulletColor = _gameplayConfig.ColorPerTeamId[_matchDataService.GetPlayer(bulletState.BelongToPlayerId).TeamId];
                _bulletControllers.CreateBullet(bulletState.Id, bulletState.Radius, bulletState.Position, bulletColor);
            }
        }

        private void CreateWalls()
        {
            var multiplier = _simulationState.MapSizeMultiplier;
            var walls = _sharedGamePlayConfig.Environment.GetEnvironmentLayout(_simulationState.EnvironmentLayoutId).GetWalls();
            if (walls.IsNullOrEmpty())
            {
                return;
            }
            
            foreach (var wall in walls)
            {
                var points = new Vector2[wall.Points.Length];
                for (int i = 0; i < points.Length; i++)
                {
                    points[i] = wall.Points[i] * multiplier;
                }
                var wallModel = _matchDataService.AddWall(wall.Id, points, Vector2.Zero, wall.Position * multiplier, 0);
                _environmentWallsControllers.CreateWall(wallModel.Id);
            }
        }
        
        private void CreateLavaWalls()
        {
            var multiplier = _simulationState.MapSizeMultiplier;
            var lavaWalls = _sharedGamePlayConfig.Environment.GetEnvironmentLayout(_simulationState.EnvironmentLayoutId).GetLavaWalls();
            if (lavaWalls.IsNullOrEmpty())
            {
                return;
            }
            
            foreach (var lavaWall in lavaWalls)
            {
                var points = new Vector2[lavaWall.Points.Length];
                for (int i = 0; i < points.Length; i++)
                {
                    points[i] = lavaWall.Points[i] * multiplier;
                }
                var lavaWallModel = _matchDataService.AddLavalWall(lavaWall.Id, points, Vector2.Zero, lavaWall.Position * multiplier, 0);
                _environmentLavaWallsControllers.CreateLavaWall(lavaWallModel.Id);
            }
        }

        private void CreateRotatingWheels()
        {
            var multiplier = _simulationState.MapSizeMultiplier;
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
                var wheelCenter = wheelConfig.CenterPosition * multiplier;
                var wallIds = wheelConfig.Walls.IsNullOrEmpty() ? new List<ushort>() : wheelConfig.Walls.Select(x => x.Id).ToList();
                var lavaWallIds = wheelConfig.LavaWalls.IsNullOrEmpty() ? new List<ushort>() : wheelConfig.LavaWalls.Select(x => x.Id).ToList();
                var springIds = wheelConfig.Springs.IsNullOrEmpty() ? new List<ushort>() : wheelConfig.Springs.Select(x => x.Id).ToList();
                var teleportGatePairIds = wheelConfig.TeleportGatePairs.IsNullOrEmpty() ? new List<ushort>() : wheelConfig.TeleportGatePairs.Select(x => x.Id).ToList();
                var wheelModel = _matchDataService.AddEnvironmentRotatingWheel(wheelConfig.Id, wheelCenter, wheelConfig.RotationSpeed, wallIds, lavaWallIds, springIds, teleportGatePairIds);
                var rotationSpeed = wheelModel.RotationSpeed;
                
                if (wheelConfig.Walls != null)
                {
                    foreach (var wallConfig in wheelConfig.Walls)
                    {
                        var scaledPosition = wallConfig.Position * multiplier;
                        EnvironmentRotatingWheelUtils.CalculateChildTransform(
                            calculationTick, rotationSpeed, deltaTime, wheelCenter, scaledPosition, 0,
                            out var worldPosition, out var worldRotation
                        );
                        
                        var points = new Vector2[wallConfig.Points.Length];
                        for (int i = 0; i < points.Length; i++)
                        {
                            points[i] = wallConfig.Points[i] * multiplier;
                        }
                        _matchDataService.AddWall(wallConfig.Id, points, scaledPosition, worldPosition, worldRotation);
                        _environmentWallsControllers.CreateWall(wallConfig.Id);
                    }
                }

                if (wheelConfig.LavaWalls != null)
                {
                    foreach (var lavaWallConfig in wheelConfig.LavaWalls)
                    {
                        var scaledPosition = lavaWallConfig.Position * multiplier;
                        EnvironmentRotatingWheelUtils.CalculateChildTransform(
                            calculationTick, rotationSpeed, deltaTime, wheelCenter, scaledPosition, 0,
                            out var worldPosition, out var worldRotation
                        );
                        
                        var points = new Vector2[lavaWallConfig.Points.Length];
                        for (int i = 0; i < points.Length; i++)
                        {
                            points[i] = lavaWallConfig.Points[i] * multiplier;
                        }
                        _matchDataService.AddLavalWall(lavaWallConfig.Id, points, scaledPosition, worldPosition, worldRotation);
                        _environmentLavaWallsControllers.CreateLavaWall(lavaWallConfig.Id);
                    }
                }

                if (wheelConfig.Springs != null)
                {
                    foreach (var springConfig in wheelConfig.Springs)
                    {
                        var scaledPosition = springConfig.Position * multiplier;
                        EnvironmentRotatingWheelUtils.CalculateChildTransform(
                            calculationTick, rotationSpeed, deltaTime, wheelCenter, scaledPosition, springConfig.RotationAngle,
                            out var worldPosition, out var worldRotation
                        );

                        _matchDataService.AddSpring(springConfig.Id, scaledPosition, worldPosition, springConfig.RotationAngle, worldRotation);
                        _environmentSpringControllers.CreateSpring(springConfig.Id);
                    }
                }

                if (wheelConfig.TeleportGatePairs != null)
                {
                    foreach (var teleportPairConfig in wheelConfig.TeleportGatePairs)
                    {
                        var gateSize = _sharedGamePlayConfig.EnvironmentTeleport.Size;

                        var scaledGateAPos = teleportPairConfig.GateA.Position * multiplier;
                        var scaledGateBPos = teleportPairConfig.GateB.Position * multiplier;

                        EnvironmentRotatingWheelUtils.CalculateChildTransform(
                            calculationTick, rotationSpeed, deltaTime, wheelCenter, scaledGateAPos, teleportPairConfig.GateA.NormalRotation,
                            out var worldPositionA, out var worldRotationA
                        );

                        EnvironmentRotatingWheelUtils.CalculateChildTransform(
                            calculationTick, rotationSpeed, deltaTime, wheelCenter, scaledGateBPos, teleportPairConfig.GateB.NormalRotation,
                            out var worldPositionB, out var worldRotationB
                        );

                        _matchDataService.AddTeleportPair(teleportPairConfig.Id, teleportPairConfig.GateAId, scaledGateAPos, teleportPairConfig.GateA.NormalRotation, teleportPairConfig.GateBId, scaledGateBPos, teleportPairConfig.GateB.NormalRotation, worldPositionA, worldRotationA, worldPositionB, worldRotationB, gateSize.ToNumericsVector2());
                        _teleportGateControllers.CreateGatePair(teleportPairConfig.Id);
                    }
                }
            }
        }
        
        private void CreateSwapField()
        {
            foreach (var swapField in _simulationState.SwapFields.AsSpan())
            {
                var casterId = swapField.PlayerCasterId;
                var casterState = _simulationState.GetPlayerById(casterId);
                var position = casterState.Spaceship.Transform.Position.ToUnityVector2();
                
                _matchDataService.AddSwapField(swapField.Id, casterId, swapField.CreatedOnTick, swapField.EndTick, swapField.Radius);
                _swapFieldControllers.CreateSwapField(swapField.Id, swapField.Radius, position);
            }
        }

        private void CreateKOPRojectiles()
        {
            foreach (var koProjectile in _simulationState.KOProjectiles.AsSpan())
            {
                var casterId = koProjectile.PlayerCasterId;
                var casterState = _simulationState.GetPlayerById(casterId);
                var position = koProjectile.Position.ToUnityVector2();
                var rotation = koProjectile.Rotation.ToUnityVector2();
                var casterPosition = casterState.Spaceship.Transform.Position.ToUnityVector2();
                
                _matchDataService.AddKOProjectile(koProjectile.Id, casterId, koProjectile.CreatedOnTick, koProjectile.Size);
                _kOProjectilesControllers.CreateKOProjectile(koProjectile.Id, position, rotation, casterPosition, koProjectile.Size);
            }
        }

        private void CreateChickenEggs()
        {
            foreach (var egg in _simulationState.ChickenEggs.AsSpan())
            {
                var casterPlayerId = egg.PlayerCasterId;
                _matchDataService.AddChickenEgg(egg.Id, casterPlayerId, egg.Position.ToUnityVector2());
                var playerCasterTeamId = _matchDataService.GetPlayerTeamId(casterPlayerId);
                _chickenEggsControllers.CreateEgg(egg.Id, egg.Position, playerCasterTeamId);
            }
        }

        private void CreateGrapplingHookPRojectiles()
        {
            foreach (var grapplingHookProjectile in _simulationState.GrapplingHookProjectiles.AsSpan())
            {
                var casterId = grapplingHookProjectile.PlayerCasterId;
                var casterState = _simulationState.GetPlayerById(casterId);
                var position = grapplingHookProjectile.Position;
                var casterPosition = casterState.Spaceship.Transform.Position.ToUnityVector2();
                var rotation = grapplingHookProjectile.Position - casterPosition.ToNumericsVector2();

                _matchDataService.AddGrapplingHookProjectile(grapplingHookProjectile.Id, casterId, position);
                _grapplingHookProjectilesControllers.CreateGrapplingHookProjectile(grapplingHookProjectile.Id, casterId, position.ToUnityVector2(), rotation.ToUnityVector2(), casterPosition, grapplingHookProjectile.IsHookAttached);
            }
        }
    }
}