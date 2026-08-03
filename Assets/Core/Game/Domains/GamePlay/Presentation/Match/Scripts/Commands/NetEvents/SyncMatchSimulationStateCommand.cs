using System.Collections.Generic;
using System.Numerics;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.Bullets.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.ChickenEggs.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.DashPulse.Scripts.Effect;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.Environment.FieldBarriers.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.Environment.LavaWalls.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.Environment.Spikes.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.Environment.Springs.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.Environment.TeleportGate.Scripts.Mvcs.EnvironmentTeleportGate;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.Environment.Walls.Scripts.Mvcs;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.GalacticPullStar.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.FrigidBlock.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.GrapplingHook.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.FishingRod.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.Soul.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.KOProjectiles.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Features.LockOnTarget;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.MagneticPullEffect.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.Mole.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.WhacAMoleCountdown.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.PreparationPhaseCountdown.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.Player.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.PowerUps.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.SecondCastAimArrowEffect.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.SwapFields.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.TalentCards.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.UI.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.UI.Scripts.TeamsBoard;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.DataService;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.StageCancellationToken;
using Core.Game.Domains.GamePlay.Presentation.Scripts.Network.PacketsHandlers;
using Core.Game.Domains.GamePlay.Presentation.Scripts.ScriptableObjects;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Shared.Scripts.Configs;
using Core.Game.Domains.GamePlay.Shared.Scripts.Enums;
using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels;
using Core.Game.Domains.GamePlay.Shared.Scripts.Utils;
using Core.Scripts.Extensions;
using Core.Scripts.Mvc.WorldCamera;
using Core.Scripts.Network;
using CoreDomain.Scripts.Services.CommandFactory;
using CoreDomain.Scripts.Services.Logger.Base;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands.NetEvents
{
    public class SyncMatchSimulationStateCommand : BaseCommand, ICommandVoid
    {
        private const float CAMERA_ORTHOGRAPHIC_SIZE_TO_MAP_SIZE_RATIO = 0.8666666667f; //1.3f / 1.5f;
        
        private IMatchDataService _matchDataService;
        private IMatchBulletControllers _bulletControllers;
        private IMatchChickenEggsControllers _chickenEggsControllers;
        private IMatchEnvironmentWallsControllers _environmentWallsControllers;
        private IEnvironmentSpringControllers _environmentSpringControllers;
        private IEnvironmentSpikeControllers _environmentSpikeControllers;
        private ITalentCardControllers _talentCardControllers;
        private SharedGamePlayConfig _sharedGamePlayConfig;
        private IEnvironmentLavaWallsControllers _environmentLavaWallsControllers;
        private IPowerUpBallControllers _powerUpBallControllers;
        private AddMatchPlayerCommand _addMatchPlayerCommand;
        private CreatePowerUpBallCommand _createPowerUpBallCommand;
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
        private IFishingRodTipControllers _fishingRodTipControllers;
        private ISecondCastAimArrowControllers _secondCastAimArrowControllers;
        private ISoulGhostControllers _soulGhostControllers;
        private IFrigidBlocksControllers _frigidBlocksControllers;
        private ILockOnTargetEffectController _lockOnTargetEffectController;
        private IPreparationPhaseCountdownController _preparationPhaseCountdownController;
        private IGalacticPullStarEffectControllers _galacticPullStarEffectControllers;
        private IMoleControllers _moleControllers;
        private IWhacAMoleCountdownController _whacAMoleCountdownController;

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
            _environmentSpikeControllers = _diContainer.Resolve<IEnvironmentSpikeControllers>();
            _environmentLavaWallsControllers = _diContainer.Resolve<IEnvironmentLavaWallsControllers>();
            _talentCardControllers = _diContainer.Resolve<ITalentCardControllers>();
            _powerUpBallControllers = _diContainer.Resolve<IPowerUpBallControllers>();
            _sharedGamePlayConfig = _diContainer.Resolve<SharedGamePlayConfig>();
            _commandFactory = _diContainer.Resolve<ICommandFactory>();
            _addMatchPlayerCommand = _commandFactory.CreateCommandVoid<AddMatchPlayerCommand>();
            _createPowerUpBallCommand = _commandFactory.CreateCommandVoid<CreatePowerUpBallCommand>();
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
            _fishingRodTipControllers = _diContainer.Resolve<IFishingRodTipControllers>();
            _secondCastAimArrowControllers = _diContainer.Resolve<ISecondCastAimArrowControllers>();
            _soulGhostControllers = _diContainer.Resolve<ISoulGhostControllers>();
            _frigidBlocksControllers = _diContainer.Resolve<IFrigidBlocksControllers>();
            _chickenEggsControllers = _diContainer.Resolve<IMatchChickenEggsControllers>();
            _lockOnTargetEffectController = _diContainer.Resolve<ILockOnTargetEffectController>();
            _preparationPhaseCountdownController = _diContainer.Resolve<IPreparationPhaseCountdownController>();
            _galacticPullStarEffectControllers = _diContainer.Resolve<IGalacticPullStarEffectControllers>();
            _moleControllers = _diContainer.Resolve<IMoleControllers>();
            _whacAMoleCountdownController = _diContainer.Resolve<IWhacAMoleCountdownController>();
        }

        public void Execute()
        {
            _matchDataService.PreperationPhaseStartedOnTick = _simulationState.PreperationPhaseStartedOnTick;
            _matchDataService.PreperationPhaseEndedOnTick = _simulationState.PreperationPhaseEndedOnTick;
            _matchDataService.IsInPreparationPhase = _simulationState.IsInPreparationPhase;
            _matchDataService.IsInShowoffWinners = _simulationState.IsInShowoffWinners;
            _matchDataService.CurrentStageWinnerTeamId = _simulationState.CurrentStageWinnerTeamId;
            _matchDataService.StageType = _simulationState.StageType;
            _matchDataService.WhacAMoleEndTick = _simulationState.WhacAMoleEndTick;
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
            _environmentSpikeControllers.DestroyAll();
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
            _fishingRodTipControllers.DestroyAll();
            _secondCastAimArrowControllers.DestroyAll();
            _soulGhostControllers.DestroyAll();
            _frigidBlocksControllers.DestroyAll();
            _chickenEggsControllers.DestroyAll();
            _galacticPullStarEffectControllers.DestroyAll();
            _lockOnTargetEffectController.DestroyAll();
            _moleControllers.DestroyAll();
            _preparationPhaseCountdownController.StopCountdown();
            _whacAMoleCountdownController.HideCountdown();
        }

        private void CreateAll()
        {
            var mapSizeMultiplier = _simulationState.MapSizeMultiplier;
            if (_simulationState.IsInShowoffWinners)
            {
                // todo handle this
            }
            else
            {
                _worldCameraController.MultiplyOthographicSize(mapSizeMultiplier * CAMERA_ORTHOGRAPHIC_SIZE_TO_MAP_SIZE_RATIO);
                _worldCameraController.SetisDampingEnabled(true);
            }

            SetTeamsData();
            CreatePlayers();
            CreateBullets();
            CreateWalls(mapSizeMultiplier);
            CreateSprings(mapSizeMultiplier);
            CreateSpikes(mapSizeMultiplier);
            CreateLavaWalls(mapSizeMultiplier);
            CreateTalentCards();
            CreatePowerUpBalls();
            CreateMoles(mapSizeMultiplier);
            CreateTeamBoards();
            var teleportGatesPerWheelId = CreateTeleportGates(mapSizeMultiplier);
            CreateRotatingWheels(mapSizeMultiplier, teleportGatesPerWheelId);
            CreateFieldBarriers(mapSizeMultiplier);
            CreateSwapField();
            CreateKOPRojectiles();
            CreateGrapplingHookPRojectiles();
            CreateFishingRodTips();
            CreateSoulGhosts();
            CreateFrigidBlocks();
            CreateChickenEggs();
            CreateGalacticPullStars();
            SetupWhacAMoleHud();
        }

        // Whac-A-Mole players cannot be damaged, so their health bars are meaningless and the moles-hit score takes over.
        private void SetupWhacAMoleHud()
        {
            var isWhacAMoleStage = _simulationState.StageType == StageType.WhacAMole;
            _teamsBoardUIController.SetIsMolesHitShown(isWhacAMoleStage);

            if (!isWhacAMoleStage)
            {
                return;
            }

            foreach (var player in _simulationState.Players.AsSpan())
            {
                _playerControllers.HidePlayerHealthBar(player.Id);
                _playerUIControllers.HidePlayerHealthBar(player.Id);
            }
        }

        // Every authored spawn point gets its own mole up front, all hiding in their holes. Server spawns only pop
        // the matching one out, so a rejoining client just has to re-pop the moles that are already out.
        private void CreateMoles(float mapSizeMultiplier)
        {
            if (_simulationState.StageType != StageType.WhacAMole)
            {
                return;
            }

            var spawnPoints = _sharedGamePlayConfig.Environment.GetEnvironmentLayout(_simulationState.EnvironmentLayoutId).GetMoleSpawnPoints();

            if (spawnPoints.IsNullOrEmpty())
            {
                LogService.LogError($"No mole spawn points authored for environment layout {_simulationState.EnvironmentLayoutId}!");
                return;
            }

            foreach (var spawnPoint in spawnPoints)
            {
                _moleControllers.CreateMoleAtSpawnPoint((spawnPoint.Position * mapSizeMultiplier).ToUnityVector2());
            }

            foreach (var mole in _simulationState.Moles.AsSpan())
            {
                var position = mole.Position.ToUnityVector2();
                _matchDataService.AddMole(mole.Id, position);
                _moleControllers.SetMoleOutsideHole(mole.Id, position);
            }
        }

        private void SetTeamsData()
        {
            foreach (var player in _simulationState.Players.AsSpan())
            {
                var playerTeamId = player.TeamId;
                _matchDataService.AddTeamIdIfDoesntExist(playerTeamId);
                var teamGems = _simulationState.GemsPerTeamId[playerTeamId];
                var teamBolts = _simulationState.BoltsPerTeam[playerTeamId];
                var teamMolesHit = _simulationState.MolesHitPerTeamId[playerTeamId];
                _matchDataService.SetTeamBolts(playerTeamId, teamBolts);
                _matchDataService.SetTeamGems(playerTeamId, teamGems);
                _matchDataService.SetTeamMolesHit(playerTeamId, teamMolesHit);
            }
        }

        private void CreateFieldBarriers(float mapSizeMultiplier)
        {
            if (!_simulationState.IsInPreparationPhase)
            {
                return;
            }
            
            var barrierConfigs = _sharedGamePlayConfig.Environment.GetEnvironmentLayout(_simulationState.EnvironmentLayoutId).GetFieldBarriers();
            if (barrierConfigs.IsNullOrEmpty())
            {
                return;
            }

            ushort barrierIndex = 0;
            foreach (var teamId in _simulationState.FieldBarriersOrderedByTeamId.AsSpan())
            {
                var barrierConfig = barrierConfigs[barrierIndex];
                _matchDataService.AddFieldBarrier(barrierIndex, teamId, barrierConfig.Position * mapSizeMultiplier, barrierConfig.Size * mapSizeMultiplier, barrierConfig.Shape);
                _environmentFieldBarrierControllers.CreateFieldBarrier(barrierIndex);
                barrierIndex++;
            }
        }

        private void CreateTeamBoards()
        {
            foreach (ushort teamId in _matchDataService.TeamIds)
            {
                var teamGems = _matchDataService.GemsPerTeam[teamId];
                var teamBolts = _matchDataService.BoltsPerTeam[teamId];
                _teamsBoardUIController.CreateTeamBoard(teamId, teamGems, teamBolts);
                _teamsBoardUIController.UpdateTeamMolesHit(teamId, _matchDataService.MolesHitPerTeam[teamId]);
            }
        }

        private void CreateSprings(float mapSizeMultiplier)
        {
            var springs = _sharedGamePlayConfig.Environment.GetEnvironmentLayout(_simulationState.EnvironmentLayoutId).GetEnvironmentSprings();
            if (springs.IsNullOrEmpty())
            {
                return;
            }
            
            foreach (var spring in springs)
            {
                _matchDataService.AddSpring(spring.Id, Vector2.Zero, spring.Position * mapSizeMultiplier, 0, spring.RotationAngle);
                _environmentSpringControllers.CreateSpring(spring.Id);
            }
        }

        private void CreateSpikes(float mapSizeMultiplier)
        {
            var spikes = _sharedGamePlayConfig.Environment.GetEnvironmentLayout(_simulationState.EnvironmentLayoutId).GetEnvironmentSpikes();
            if (spikes.IsNullOrEmpty())
            {
                return;
            }

            foreach (var spike in spikes)
            {
                _matchDataService.AddSpike(spike.Id, Vector2.Zero, spike.Position * mapSizeMultiplier, 0, spike.RotationAngle);
                _environmentSpikeControllers.CreateSpike(spike.Id);
            }
        }
        
        private Dictionary<ushort, List<RotatingTeleportGate>> CreateTeleportGates(float mapSizeMultiplier)
        {
            var teleportGatesPerWheelId = new Dictionary<ushort, List<RotatingTeleportGate>>();
            
            var layout = _sharedGamePlayConfig.Environment.GetEnvironmentLayout(_simulationState.EnvironmentLayoutId);
            var teleportGatePairConfigs = layout.GetTeleportGates();
            if (teleportGatePairConfigs.IsNullOrEmpty())
            {
                return teleportGatesPerWheelId;
            }

            var wheelsDict = GetRotatingWheelsDictionary(layout);
            var calculationTick = GetTicksPassedSincePreparationPhaseEneded();
            var deltaTime = _networkConfig.DeltaTime;
            var gateSize = _sharedGamePlayConfig.EnvironmentTeleport.Size;

            foreach (var pairConfig in teleportGatePairConfigs)
            {
                TryAttachTeleportGateToRotatingWheel(pairConfig.Id, pairConfig.GateA, true, mapSizeMultiplier, calculationTick, deltaTime, wheelsDict, teleportGatesPerWheelId, out var worldPosA, out var worldRotA);
                TryAttachTeleportGateToRotatingWheel(pairConfig.Id, pairConfig.GateB, false, mapSizeMultiplier, calculationTick, deltaTime, wheelsDict, teleportGatesPerWheelId, out var worldPosB, out var worldRotB);

                var scaledGateAPos = pairConfig.GateA.Position * mapSizeMultiplier;
                var scaledGateBPos = pairConfig.GateB.Position * mapSizeMultiplier;
                var scaledGateSize = gateSize.ToNumericsVector2() * mapSizeMultiplier;

                _matchDataService.AddTeleportPair(
                    pairConfig.Id, 
                    pairConfig.GateAId, scaledGateAPos, pairConfig.GateA.NormalRotation, 
                    pairConfig.GateBId, scaledGateBPos, pairConfig.GateB.NormalRotation, 
                    worldPosA, worldRotA, 
                    worldPosB, worldRotB, 
                    scaledGateSize
                );
                
                _teleportGateControllers.CreateGatePair(pairConfig.Id);
            }

            return teleportGatesPerWheelId;
        }

        private Dictionary<ushort, EnvironmentRotatingWheelConfig> GetRotatingWheelsDictionary(EnvironmentLayoutConfig layout)
        {
            var wheelsDict = new Dictionary<ushort, EnvironmentRotatingWheelConfig>();
            var rotatingWheelsConfigs = layout.GetRotatingWheels();
            
            if (!rotatingWheelsConfigs.IsNullOrEmpty())
            {
                foreach (var wheel in rotatingWheelsConfigs)
                {
                    wheelsDict[wheel.Id] = wheel;
                }
            }
            return wheelsDict;
        }

        private int GetTicksPassedSincePreparationPhaseEneded()
        {
            if (_matchDataService.IsInPreparationPhase)
            {
                return 0;
            }
            return _fullTickPacketsHandler.LastProcessedTickFromServer - _matchDataService.PreperationPhaseEndedOnTick;
        }

        private void TryAttachTeleportGateToRotatingWheel(
            ushort gatePairId,
            EnvironmentTeleportGateConfig gateConfig,
            bool isGateA,
            float mapSizeMultiplier,
            int calculationTick,
            float deltaTime,
            Dictionary<ushort, EnvironmentRotatingWheelConfig> wheelsDict,
            Dictionary<ushort, List<RotatingTeleportGate>> teleportGatesPerWheelId,
            out Vector2 worldPosition,
            out float worldRotation)
        {
            var scaledPosition = gateConfig.Position * mapSizeMultiplier;
            worldPosition = scaledPosition;
            worldRotation = gateConfig.NormalRotation;

            if (gateConfig.IsAttachedToRotationWheel && wheelsDict.TryGetValue(gateConfig.AttachToRotationWheelId, out var wheel))
            {
                EnvironmentRotatingWheelUtils.CalculateChildTransform(
                    calculationTick, 
                    wheel.RotationSpeed, 
                    deltaTime, 
                    wheel.CenterPosition * mapSizeMultiplier, 
                    scaledPosition, 
                    gateConfig.NormalRotation, 
                    out worldPosition, 
                    out worldRotation
                );

                if (!teleportGatesPerWheelId.TryGetValue(wheel.Id, out var gateList))
                {
                    gateList = new List<RotatingTeleportGate>();
                    teleportGatesPerWheelId[wheel.Id] = gateList;
                }
                gateList.Add(new RotatingTeleportGate(gatePairId, isGateA));
            }
        }
        
        private void CreatePowerUpBalls()
        {
            foreach (var powerUpBall in _simulationState.PowerUpBalls.AsSpan())
            {
                var position = powerUpBall.Position.ToUnityVector2();
                _matchDataService.AddPowerUpBall(powerUpBall.Id, position);
                _createPowerUpBallCommand.SetPowerUpBallId(powerUpBall.Id).SetPosition(position).Execute();
            }
        }
        
        private void CreateTalentCards()
        {
            foreach (var talentCard in _simulationState.TalentCards.AsSpan())
            {
                _matchDataService.AddTalentCard(talentCard.Id, talentCard.Position.ToUnityVector2(), talentCard.TalentType, talentCard.Health);// NOTE: talentCard.Position is already scaled by the server (InitStageCommand stores position * mapSizeMultiplier into state), so it must not be scaled again here.
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

        private void CreateWalls(float mapSizeMultiplier)
        {
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
                    points[i] = wall.Points[i] * mapSizeMultiplier;
                }
                var wallModel = _matchDataService.AddWall(wall.Id, points, Vector2.Zero, wall.Position * mapSizeMultiplier, 0);
                _environmentWallsControllers.CreateWall(wallModel.Id);
            }
        }
        
        private void CreateLavaWalls(float mapSizeMultiplier)
        {
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
                    points[i] = lavaWall.Points[i] * mapSizeMultiplier;
                }
                var lavaWallModel = _matchDataService.AddLavalWall(lavaWall.Id, points, Vector2.Zero, lavaWall.Position * mapSizeMultiplier, 0);
                _environmentLavaWallsControllers.CreateLavaWall(lavaWallModel.Id);
            }
        }

        private void CreateRotatingWheels(float mapSizeMultiplier, Dictionary<ushort, List<RotatingTeleportGate>> teleportGatesPerWheelId)
        {
            var wheels = _sharedGamePlayConfig.Environment.GetEnvironmentLayout(_simulationState.EnvironmentLayoutId).GetRotatingWheels();
            if (wheels.IsNullOrEmpty())
            {
                return;
            }
            var calculationTick = GetTicksPassedSincePreparationPhaseEneded();
            var deltaTime = _networkConfig.DeltaTime;

            foreach (var wheelConfig in wheels)
            {
                var wheelCenter = wheelConfig.CenterPosition * mapSizeMultiplier;
                var wallIds = wheelConfig.Walls.IsNullOrEmpty() ? new List<ushort>() : wheelConfig.Walls.Select(x => x.Id).ToList();
                var lavaWallIds = wheelConfig.LavaWalls.IsNullOrEmpty() ? new List<ushort>() : wheelConfig.LavaWalls.Select(x => x.Id).ToList();
                var springIds = wheelConfig.Springs.IsNullOrEmpty() ? new List<ushort>() : wheelConfig.Springs.Select(x => x.Id).ToList();
                var spikeIds = wheelConfig.Spikes.IsNullOrEmpty() ? new List<ushort>() : wheelConfig.Spikes.Select(x => x.Id).ToList();
                
                var teleportGatePairIds = new List<RotatingTeleportGate>();
                if (teleportGatesPerWheelId != null && teleportGatesPerWheelId.TryGetValue(wheelConfig.Id, out var gates)) {
                    teleportGatePairIds = gates;
                }

                var wheelModel = _matchDataService.AddEnvironmentRotatingWheel(wheelConfig.Id, wheelCenter, wheelConfig.RotationSpeed, wallIds, lavaWallIds, springIds, spikeIds, teleportGatePairIds);
                var rotationSpeed = wheelModel.RotationSpeed;
                
                if (wheelConfig.Walls != null)
                {
                    foreach (var wallConfig in wheelConfig.Walls)
                    {
                        var scaledPosition = wallConfig.Position * mapSizeMultiplier;
                        EnvironmentRotatingWheelUtils.CalculateChildTransform(
                            calculationTick, rotationSpeed, deltaTime, wheelCenter, scaledPosition, 0,
                            out var worldPosition, out var worldRotation
                        );
                        
                        var points = new Vector2[wallConfig.Points.Length];
                        for (int i = 0; i < points.Length; i++)
                        {
                            points[i] = wallConfig.Points[i] * mapSizeMultiplier;
                        }
                        _matchDataService.AddWall(wallConfig.Id, points, scaledPosition, worldPosition, worldRotation);
                        _environmentWallsControllers.CreateWall(wallConfig.Id);
                    }
                }

                if (wheelConfig.LavaWalls != null)
                {
                    foreach (var lavaWallConfig in wheelConfig.LavaWalls)
                    {
                        var scaledPosition = lavaWallConfig.Position * mapSizeMultiplier;
                        EnvironmentRotatingWheelUtils.CalculateChildTransform(
                            calculationTick, rotationSpeed, deltaTime, wheelCenter, scaledPosition, 0,
                            out var worldPosition, out var worldRotation
                        );
                        
                        var points = new Vector2[lavaWallConfig.Points.Length];
                        for (int i = 0; i < points.Length; i++)
                        {
                            points[i] = lavaWallConfig.Points[i] * mapSizeMultiplier;
                        }
                        _matchDataService.AddLavalWall(lavaWallConfig.Id, points, scaledPosition, worldPosition, worldRotation);
                        _environmentLavaWallsControllers.CreateLavaWall(lavaWallConfig.Id);
                    }
                }

                if (wheelConfig.Springs != null)
                {
                    foreach (var springConfig in wheelConfig.Springs)
                    {
                        var scaledPosition = springConfig.Position * mapSizeMultiplier;
                        EnvironmentRotatingWheelUtils.CalculateChildTransform(
                            calculationTick, rotationSpeed, deltaTime, wheelCenter, scaledPosition, springConfig.RotationAngle,
                            out var worldPosition, out var worldRotation
                        );

                        _matchDataService.AddSpring(springConfig.Id, scaledPosition, worldPosition, springConfig.RotationAngle, worldRotation);
                        _environmentSpringControllers.CreateSpring(springConfig.Id);
                    }
                }

                if (wheelConfig.Spikes != null)
                {
                    foreach (var spikeConfig in wheelConfig.Spikes)
                    {
                        var scaledPosition = spikeConfig.Position * mapSizeMultiplier;
                        EnvironmentRotatingWheelUtils.CalculateChildTransform(
                            calculationTick, rotationSpeed, deltaTime, wheelCenter, scaledPosition, spikeConfig.RotationAngle,
                            out var worldPosition, out var worldRotation
                        );

                        _matchDataService.AddSpike(spikeConfig.Id, scaledPosition, worldPosition, spikeConfig.RotationAngle, worldRotation);
                        _environmentSpikeControllers.CreateSpike(spikeConfig.Id);
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

        private void CreateFrigidBlocks()
        {
            foreach (var frigidBlock in _simulationState.FrigidBlocks.AsSpan())
            {
                _matchDataService.AddFrigidBlock(frigidBlock.Id, frigidBlock.PlayerCasterId, frigidBlock.Position, frigidBlock.Rotation);
                _frigidBlocksControllers.CreateFrigidBlock(frigidBlock.Id, frigidBlock.Position.ToUnityVector2(), frigidBlock.Rotation.ToUnityVector2());
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

        private void CreateGalacticPullStars()
        {
            foreach (var field in _simulationState.GalacticForceFields.AsSpan())
            {
                _galacticPullStarEffectControllers.ShowStar(field.Id, field.CasterTeamId);
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
                _grapplingHookProjectilesControllers.CreateGrapplingHookProjectile(grapplingHookProjectile.Id, casterId, position.ToUnityVector2(), rotation.ToUnityVector2(), casterPosition, grapplingHookProjectile.HitData.IsHookAttached);
            }
        }

        private void CreateSoulGhosts()
        {
            foreach (var soulGhost in _simulationState.SoulGhosts.AsSpan())
            {
                var casterId = soulGhost.PlayerCasterId;
                var casterState = _simulationState.GetPlayerById(casterId);
                _matchDataService.AddSoulGhost(soulGhost.Id, casterId, soulGhost.Position, soulGhost.Direction);
                _soulGhostControllers.CreateSoulGhost(soulGhost.Id, casterId, casterState.TeamId, soulGhost.Position.ToUnityVector2(), soulGhost.Direction.ToUnityVector2());
            }
        }

        private void CreateFishingRodTips()
        {
            foreach (var fishingRodTip in _simulationState.FishingRodProjectiles.AsSpan())
            {
                var casterId = fishingRodTip.PlayerCasterId;
                var casterState = _simulationState.GetPlayerById(casterId);
                var position = fishingRodTip.Position;
                var casterPosition = casterState.Spaceship.Transform.Position.ToUnityVector2();
                var rotation = fishingRodTip.Position - casterPosition.ToNumericsVector2();
                _matchDataService.AddFishingRodTip(fishingRodTip.Id, casterId, position, fishingRodTip.Phase);
                _fishingRodTipControllers.CreateFishingRodTip(fishingRodTip.Id, position.ToUnityVector2(), rotation.ToUnityVector2(), casterPosition, fishingRodTip.Phase);

                if (fishingRodTip.Phase == FishingRodTipPhase.CaughtEnemy)
                {
                    _secondCastAimArrowControllers.AddArrow(fishingRodTip.Id, position.ToUnityVector2(), fishingRodTip.EnemyCaughtArrowDirection.ToUnityVector2());
                }
            }
        }
    }
}