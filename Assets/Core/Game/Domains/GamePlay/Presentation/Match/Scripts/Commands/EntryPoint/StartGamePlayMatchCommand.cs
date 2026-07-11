using System.Threading;
using Core.Game.Domains.GamePlay.Presentation.Features.Environment.Background.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.Bullets.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.ChickenEggs.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.DashPulse.Scripts.Effect;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.Environment.FieldBarriers.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.Environment.LavaWalls.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.Environment.Spikes.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.Environment.Springs.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.Environment.TeleportGate.Scripts.Mvcs.EnvironmentTeleportGate;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.Environment.TeleportGate.Scripts.Mvcs.PlayerTeleportEffect;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.Environment.Walls.Scripts.Mvcs;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.GainBoltEffect.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.GalacticPullStar.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.HitDamageIndicatorEffect.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.KOProjectiles.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.GrapplingHook.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.FishingRod.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.Soul.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.FrigidBlock.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.HeadbuttHitEffect.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Features.LockOnTarget;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.MagneticPullEffect.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.Nuke.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.Player.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.PowerUps.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.PowerUps.Scripts.ObtainedEffect;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.SecondCastAimArrowEffect.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.SwapFields.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.TalentCards.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.TalentCards.Scripts.ObtainedEffect;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands.NetEvents;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.DataService;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Initiator;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Network.PacketsHandlers;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.TickProcessor;
using Core.Game.Domains.GamePlay.Presentation.Scripts.GameInputActions;
using Core.Game.Domains.GamePlay.Presentation.Scripts.Network.PacketsHandlers;
using Core.Game.Domains.GamePlay.Presentation.Scripts.Services.DataService;
using Core.Game.Domains.GamePlay.Presentation.Scripts.TickProcessors;
using Core.Scripts.Services.AudioService;
using CoreDomain.Scripts.Services.CommandFactory;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands.EntryPoint
{
    public class StartGamePlayMatchCommand: BaseCommand, ICommandVoid
    {
        private GamePlayMatchInitiatorEnterData _enterData;
        private ITalentCardObtainedEffectController _talentCardObtainedEffectController;
        private IEnvironmentLavaWallsControllers _environmentLavaWallsControllers;
        private IPowerUpBallControllers _powerUpBallControllers;
        private IPowerUpBallObtainedEffectController _powerUpBallObtainedEffectController;
        private ITalentCardControllers _talentCardControllers;
        private IMatchPlayerControllers _playerControllers;
        private IMatchBulletControllers _bulletControllers;
        private IMatchChickenEggsControllers _chickenEggsControllers;
        private IGalacticPullStarEffectControllers _galacticPullStarEffectControllers;
        private ITickProcessor _tickProcessor;
        private IMatchEnvironmentWallsControllers _environmentWallsControllers;
        private IEnvironmentSpringControllers _environmentSpringControllers;
        private IEnvironmentSpikeControllers _environmentSpikeControllers;
        private IFullTickPacketsHandler _fullTickPacketsHandler;
        private IMatchDataService _matchDataService;
        private ICommandFactory _commandFactory;
        private IClientMatchPresentationTickProcessor _clientMatchPresentationTickProcessor;
        private IStartStagePacketHandler _startStagePacketHandler;
        private IGainBoltEffectController _gainBoltEffectController;
        private IHitDamageIndicatorEffectController _hitDamageIndicatorEffectController;
        private IPlayerTeleportEffectController _playerTeleportEffectController;
        private IHeadbuttHitEffectController _headbuttHitEffectController;
        private IEnvironmentTeleportGateControllers _environmentTeleportGateControllers;
        private IEnvironmentFieldBarrierControllers _environmentFieldBarrierControllers;
        private ISwapFieldControllers _swapFieldControllers;
        private IKOProjectilesControllers _koProjectilesControllers;
        private IGrapplingHookProjectilesControllers _grapplingHookProjectilesControllers;
        private IFishingRodTipControllers _fishingRodTipControllers;
        private ISecondCastEffectController _secondCastEffectController;
        private ISoulGhostControllers _soulGhostControllers;
        private IFrigidBlocksControllers _frigidBlocksControllers;
        private IDashPulseGustEffectController _dashPulseGustEffectController;
        private IMagneticPullEffectController _magneticPullEffectController;
        private IBackgroundParallaxController _backgroundParallaxController;
        private ILockOnTargetEffectController _lockOnTargetEffectController;
        private ILockOnTargetShootEffectController _lockOnTargetShootEffectController;
        private ILocalPlayersDataService _localPlayersDataService;
        private IGameInputActionsController _gameInputActionsController;
        private IAudioService _audioService;
        private INukeShockwaveEffectController _nukeShockwaveEffectController;

        public StartGamePlayMatchCommand SetEnterData(GamePlayMatchInitiatorEnterData enterEnterData)
        {
            _enterData = enterEnterData;
            return this;
        }

        public override void ResolveDependencies()
        {
            _environmentLavaWallsControllers = _diContainer.Resolve<IEnvironmentLavaWallsControllers>();
            _talentCardObtainedEffectController = _diContainer.Resolve<ITalentCardObtainedEffectController>();
            _powerUpBallControllers = _diContainer.Resolve<IPowerUpBallControllers>();
            _powerUpBallObtainedEffectController = _diContainer.Resolve<IPowerUpBallObtainedEffectController>();
            _talentCardControllers = _diContainer.Resolve<ITalentCardControllers>();
            _playerControllers = _diContainer.Resolve<IMatchPlayerControllers>();
            _bulletControllers = _diContainer.Resolve<IMatchBulletControllers>();
            _chickenEggsControllers = _diContainer.Resolve<IMatchChickenEggsControllers>();
            _galacticPullStarEffectControllers = _diContainer.Resolve<IGalacticPullStarEffectControllers>();
            _tickProcessor = _diContainer.Resolve<ITickProcessor>();
            _environmentWallsControllers = _diContainer.Resolve<IMatchEnvironmentWallsControllers>();
            _environmentSpringControllers = _diContainer.Resolve<IEnvironmentSpringControllers>();
            _environmentSpikeControllers = _diContainer.Resolve<IEnvironmentSpikeControllers>();
            _fullTickPacketsHandler = _diContainer.Resolve<IFullTickPacketsHandler>();
            _matchDataService = _diContainer.Resolve<IMatchDataService>();
            _commandFactory = _diContainer.Resolve<ICommandFactory>();
            _clientMatchPresentationTickProcessor = _diContainer.Resolve<IClientMatchPresentationTickProcessor>();
            _startStagePacketHandler = _diContainer.Resolve<IStartStagePacketHandler>();
            _gainBoltEffectController = _diContainer.Resolve<IGainBoltEffectController>();
            _hitDamageIndicatorEffectController = _diContainer.Resolve<IHitDamageIndicatorEffectController>();
            _playerTeleportEffectController = _diContainer.Resolve<IPlayerTeleportEffectController>();
            _headbuttHitEffectController = _diContainer.Resolve<IHeadbuttHitEffectController>();
            _environmentTeleportGateControllers = _diContainer.Resolve<IEnvironmentTeleportGateControllers>();
            _environmentFieldBarrierControllers = _diContainer.Resolve<IEnvironmentFieldBarrierControllers>();
            _swapFieldControllers = _diContainer.Resolve<ISwapFieldControllers>();
            _koProjectilesControllers = _diContainer.Resolve<IKOProjectilesControllers>();
            _grapplingHookProjectilesControllers = _diContainer.Resolve<IGrapplingHookProjectilesControllers>();
            _fishingRodTipControllers = _diContainer.Resolve<IFishingRodTipControllers>();
            _secondCastEffectController = _diContainer.Resolve<ISecondCastEffectController>();
            _soulGhostControllers = _diContainer.Resolve<ISoulGhostControllers>();
            _frigidBlocksControllers = _diContainer.Resolve<IFrigidBlocksControllers>();
            _dashPulseGustEffectController = _diContainer.Resolve<IDashPulseGustEffectController>();
            _magneticPullEffectController = _diContainer.Resolve<IMagneticPullEffectController>();
            _backgroundParallaxController = _diContainer.Resolve<IBackgroundParallaxController>();
            _lockOnTargetEffectController = _diContainer.Resolve<ILockOnTargetEffectController>();
            _lockOnTargetShootEffectController = _diContainer.Resolve<ILockOnTargetShootEffectController>();
            _localPlayersDataService = _diContainer.Resolve<ILocalPlayersDataService>();
            _gameInputActionsController = _diContainer.Resolve<IGameInputActionsController>();
            _audioService = _diContainer.Resolve<IAudioService>();
            _nukeShockwaveEffectController = _diContainer.Resolve<INukeShockwaveEffectController>();
        }

        public void Execute()
        {
            _audioService.PlayAudioLoop(AudioClipType.MatchGamePlayBGMusic);
            _fullTickPacketsHandler.InitEntryPoint();
            _startStagePacketHandler.InitEntryPoint();
            _talentCardControllers.InitEntryPoint();
            _environmentLavaWallsControllers.InitEntryPoint();
            _environmentFieldBarrierControllers.InitEntryPoint();
            _talentCardObtainedEffectController.InitEntryPoint();
            _powerUpBallControllers.InitEntryPoint();
            _swapFieldControllers.InitEntryPoint();
            _koProjectilesControllers.InitEntryPoint();
            _grapplingHookProjectilesControllers.InitEntryPoint();
            _fishingRodTipControllers.InitEntryPoint();
            _secondCastEffectController.InitEntryPoint();
            _soulGhostControllers.InitEntryPoint();
            _frigidBlocksControllers.InitEntryPoint();
            _powerUpBallObtainedEffectController.InitEntryPoint();
            _playerControllers.InitEntryPoint();
            _bulletControllers.InitEntryPoint();
            _chickenEggsControllers.InitEntryPoint();
            _nukeShockwaveEffectController.InitEntryPoint();
            _galacticPullStarEffectControllers.InitEntryPoint();
            _environmentWallsControllers.InitEntryPoint();
            _environmentSpringControllers.InitEntryPoint();
            _environmentSpikeControllers.InitEntryPoint();
            _playerTeleportEffectController.InitEntryPoint();
            _headbuttHitEffectController.InitEntryPoint();
            _environmentTeleportGateControllers.InitEntryPoint();
            _backgroundParallaxController.InitEntryPoint();
            _lockOnTargetEffectController.InitEntryPoint();
            _lockOnTargetShootEffectController.InitEntryPoint();
            _commandFactory.CreateCommandVoid<SyncMatchSimulationStateCommand>()
                 .SetSimulationState(_enterData.InitialState)
                 .SetOccuredOnTick(_enterData.StateOccouredOnTick)
                 .Execute();
            AddPlayersDevicesNotAddedDuringMatchMaking(); // in case we entered from playback
            _gainBoltEffectController.InitEntryPoint();
            _hitDamageIndicatorEffectController.InitEntryPoint();
            _dashPulseGustEffectController.InitEntryPoint();
            _magneticPullEffectController.InitEntryPoint();
            _tickProcessor.InitEntryPoint();
            _clientMatchPresentationTickProcessor.InitEntryPoint();
        }
        
        private void AddPlayersDevicesNotAddedDuringMatchMaking()
        {
            _localPlayersDataService.SetLocalPlayers(_enterData.PlayerIdToDeviceIdDictionary);

            foreach (var kvp in _localPlayersDataService.GetPlayerIdToDeviceIdDictionary())
            {
                _gameInputActionsController.AddPlayerIfNotAlreadyExist(kvp.Key,_localPlayersDataService.GetInputDeviceForPlayer(kvp.Key));
            }
        }
    }
}
