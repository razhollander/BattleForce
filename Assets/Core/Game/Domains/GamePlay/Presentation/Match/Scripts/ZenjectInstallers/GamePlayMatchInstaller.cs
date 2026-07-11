using Core.Game.Domains.GamePlay.Presentation.Features.Bullets.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Features.ChickenEggs.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Features.Environment.Background.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Features.Environment.Walls.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Features.Player.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.Bullets.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.ChickenEggs.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.DashPulse.Scripts.Effect;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.Environment.FieldBarriers.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.Environment.LavaWalls.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.Environment.Springs.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.Environment.Springs.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.Environment.Spikes.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.Environment.TeleportGate.Scripts.Mvcs.EnvironmentTeleportGate;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.Environment.TeleportGate.Scripts.Mvcs.PlayerTeleportEffect;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.Environment.Walls.Scripts.Mvcs;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.GainBoltEffect.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.GalacticPullStar.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.GalacticPullStar.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.HitDamageIndicatorEffect.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.PreparationPhaseCountdown.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.Player.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.PowerUps.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.PowerUps.Scripts.ObtainedEffect;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.SwapFields.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.KOProjectiles.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.GrapplingHook.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.FishingRod.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.Soul.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.FrigidBlock.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.FrigidBlock.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.HeadbuttHitEffect.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.MagneticPullEffect.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.Nuke.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Features.LockOnTarget;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.SecondCastAimArrowEffect.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.TalentCards.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.TalentCards.Scripts.ObtainedEffect;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.UI.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.UI.Scripts.TeamsBoard;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.DataService;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Initiator;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Network.PacketsHandlers;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.StageCancellationToken;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.TickProcessor;
using Core.Game.Domains.GamePlay.Presentation.Scripts.Network.PacketsHandlers;
using Core.Game.Domains.GamePlay.Presentation.Scripts.TickProcessors;
using UnityEngine;
using Zenject;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.ZenjectInstallers
{
    public class GamePlayMatchInstaller : MonoInstaller
    {
        [SerializeField] private MatchPlayerView _playerViewPrefab;
        [SerializeField] private EnvironmentLavaWallView _environmentLavaWallViewPrefab;
        [SerializeField] private TalentCardView _talentCardViewPrefab;
        [SerializeField] private TalentCardObtainedEffectView _talentCardObtainedEffectView;
        [SerializeField] private PowerUpBallView _powerUpBallViewPrefab;
        [SerializeField] private PowerUpBallObtainedEffectView _powerUpBallObtainedEffectViewPrefab;
        [SerializeField] private MatchPlayerUIControllersView _matchPlayerUIControllersView;
        [SerializeField] private BulletView _bulletViewPrefab;
        [SerializeField] private EnvironmentWallView _environmentWallViewPrefab;
        [SerializeField] private EnvironmentSpringView _environmentSpringViewPrefab;
        [SerializeField] private Core.Game.Domains.GamePlay.Presentation.Match.Features.Environment.Spikes.Scripts.EnvironmentSpikeView _environmentSpikeViewPrefab;
        [SerializeField] private EnvironmentTeleportGateView _environmentTeleportGateViewPrefab;
        [SerializeField] private StageEndedUiView _stageEndedUiView;
        [SerializeField] private TeamsBoardContainerView _teamsBoardContainerView;
        [SerializeField] private GainBoltEffectView gainBoltEffectViewPrefab;
        [SerializeField] private HitDamageIndicatorEffectView _hitDamageIndicatorEffectViewPrefab;
        [SerializeField] private PreparationPhaseCountdownView _preparationPhaseCountdownView;
        [SerializeField] private PlayerTeleportEffectView playerTeleportEffectViewPrefab;
        [SerializeField] private HeadbuttHitEffectView _headbuttHitEffectViewPrefab;
        [SerializeField] private EnvironmentFieldBarrierView _environmentFieldBarrierViewPrefab;
        [SerializeField] private SwapFieldView _swapFieldViewPrefab;
        [SerializeField] private KOProjectileView _koProjectileViewPrefab;
        [SerializeField] private GrapplingHookProjectileView _grapplingHookProjectileViewPrefab;
        [SerializeField] private FishingRodTipView _fishingRodTipViewPrefab;
        [SerializeField] private SecondCastAimArrowView _secondCastAimArrowViewPrefab;
        [SerializeField] private SoulGhostView _soulGhostViewPrefab;
        [SerializeField] private FrigidBlockView _frigidBlockViewPrefab;
        [SerializeField] private DashPulseGustEffectView _dashPulseGustEffectViewPrefab;
        [SerializeField] private NukeShockwaveEffectView _nukeShockwaveEffectViewPrefab;
        [SerializeField] private MagneticPullFieldView _magneticPullFieldViewPrefab;
        [SerializeField] private MagneticPullHitEffectView _magneticPullHitEffectViewPrefab;
        [SerializeField] private LockOnTargetEffectView lockOnTargetEffectViewPrefab;
        [SerializeField] private LockOnTargetShootEffectView lockOnTargetShootEffectViewPrefab;
        [SerializeField] private ChickenEggView _chickenEggViewPrefab;
        [SerializeField] private GalacticPullStarEffectView _galacticPullStarEffectViewPrefab;
        [SerializeField] private GalacticStarsVisualData _galacticStarsVisualData;
        [SerializeField] private BackgroundParallaxView _backgroundParallaxView;

        public override void InstallBindings()
        {
            BindServices();
            BindControllers();
        }

        private void BindServices()
        {
            Container.Bind<IGamePlayMatchInitiator>().To<GamePlayMatchInitiator>().AsSingle().NonLazy();
            Container.Bind<IMatchDataService>().To<MatchDataService>().AsSingle().NonLazy();
            Container.Bind<IStageCancellationTokenProvider>().To<StageCancellationTokenProvider>().AsSingle().NonLazy();
            Container.Rebind<ITickProcessor>().To<ClientMatchNetworkTickProcessor>().AsSingle().NonLazy();
            Container.Bind<IFullTickPacketsHandler>().To<MatchFullTickPacketsHandler>().AsSingle().NonLazy();
            Container.Bind<IClientMatchPresentationTickProcessor>().To<ClientMatchPresentationTickProcessor>().AsSingle().NonLazy();
            Container.Bind<IStartStagePacketHandler>().To<StartStagePacketHandler>().AsSingle().NonLazy();
        }
        
        private void BindControllers()
        {
            Container.BindInterfacesTo<EnvironmentLavaWallsControllers>().AsSingle().WithArguments(_environmentLavaWallViewPrefab).NonLazy();
            Container.BindInterfacesTo<MatchPlayerControllers>().AsSingle().WithArguments(_playerViewPrefab).NonLazy();
            Container.BindInterfacesTo<TalentCardControllers>().AsSingle().WithArguments(_talentCardViewPrefab).NonLazy();
            Container.BindInterfacesTo<TalentCardObtainedEffectController>().AsSingle().WithArguments(_talentCardObtainedEffectView).NonLazy();
            Container.BindInterfacesTo<PowerUpBallControllers>().AsSingle().WithArguments(_powerUpBallViewPrefab).NonLazy();
            Container.BindInterfacesTo<SwapFieldControllers>().AsSingle().WithArguments(_swapFieldViewPrefab).NonLazy();
            Container.BindInterfacesTo<PowerUpBallObtainedEffectController>().AsSingle().WithArguments(_powerUpBallObtainedEffectViewPrefab).NonLazy();
            Container.BindInterfacesTo<MatchPlayerUIControllers>().AsSingle().WithArguments(_matchPlayerUIControllersView).NonLazy();
            Container.BindInterfacesTo<MatchBulletControllers>().AsSingle().WithArguments(_bulletViewPrefab).NonLazy();
            Container.BindInterfacesTo<MatchChickenEggsControllers>().AsSingle().WithArguments(_chickenEggViewPrefab).NonLazy();
            Container.BindInterfacesTo<GalacticPullStarEffectControllers>().AsSingle().WithArguments(_galacticPullStarEffectViewPrefab, _galacticStarsVisualData).NonLazy();
            Container.BindInterfacesTo<MatchEnvironmentWallsControllers>().AsSingle().WithArguments(_environmentWallViewPrefab).NonLazy();
            Container.BindInterfacesTo<EnvironmentSpringControllers>().AsSingle().WithArguments(_environmentSpringViewPrefab).NonLazy();
            Container.BindInterfacesTo<EnvironmentSpikeControllers>().AsSingle().WithArguments(_environmentSpikeViewPrefab).NonLazy();
            Container.BindInterfacesTo<StageEndedUiController>().AsSingle().WithArguments(_stageEndedUiView).NonLazy();
            Container.BindInterfacesTo<TeamsBoardUIController>().AsSingle().WithArguments(_teamsBoardContainerView).NonLazy();
            Container.BindInterfacesTo<GainBoltEffectController>().AsSingle().WithArguments(gainBoltEffectViewPrefab).NonLazy();
            Container.BindInterfacesTo<HitDamageIndicatorEffectController>().AsSingle().WithArguments(_hitDamageIndicatorEffectViewPrefab).NonLazy();
            Container.BindInterfacesTo<PreparationPhaseCountdownController>().AsSingle().WithArguments(_preparationPhaseCountdownView).NonLazy();
            Container.BindInterfacesTo<EnvironmentTeleportGateControllers>().AsSingle().WithArguments(_environmentTeleportGateViewPrefab).NonLazy();
            Container.BindInterfacesTo<PlayerTeleportEffectController>().AsSingle().WithArguments(playerTeleportEffectViewPrefab).NonLazy();
            Container.BindInterfacesTo<HeadbuttHitEffectController>().AsSingle().WithArguments(_headbuttHitEffectViewPrefab).NonLazy();
            Container.BindInterfacesTo<EnvironmentFieldBarrierControllers>().AsSingle().WithArguments(_environmentFieldBarrierViewPrefab).NonLazy();
            Container.BindInterfacesTo<KOProjectilesControllers>().AsSingle().WithArguments(_koProjectileViewPrefab).NonLazy();
            Container.BindInterfacesTo<GrapplingHookProjectilesControllers>().AsSingle().WithArguments(_grapplingHookProjectileViewPrefab).NonLazy();
            Container.BindInterfacesTo<FishingRodTipControllers>().AsSingle().WithArguments(_fishingRodTipViewPrefab).NonLazy();
            Container.BindInterfacesTo<SecondCastEffectController>().AsSingle().WithArguments(_secondCastAimArrowViewPrefab).NonLazy();
            Container.BindInterfacesTo<SoulGhostControllers>().AsSingle().WithArguments(_soulGhostViewPrefab).NonLazy();
            Container.BindInterfacesTo<FrigidBlocksControllers>().AsSingle().WithArguments(_frigidBlockViewPrefab).NonLazy();
            Container.BindInterfacesTo<DashPulseGustEffectController>().AsSingle().WithArguments(_dashPulseGustEffectViewPrefab).NonLazy();
            Container.BindInterfacesTo<NukeShockwaveEffectController>().AsSingle().WithArguments(_nukeShockwaveEffectViewPrefab).NonLazy();
            Container.BindInterfacesTo<MagneticPullEffectController>().AsSingle().WithArguments(_magneticPullFieldViewPrefab, _magneticPullHitEffectViewPrefab).NonLazy();
            Container.BindInterfacesTo<BackgroundParallaxController>().AsSingle().WithArguments(_backgroundParallaxView).NonLazy();
            Container.BindInterfacesTo<PlayersLockOnTargetEffectControllers>().AsSingle().WithArguments(lockOnTargetEffectViewPrefab).NonLazy();
            Container.BindInterfacesTo<LockOnTargetShootEffectController>().AsSingle().WithArguments(lockOnTargetShootEffectViewPrefab).NonLazy();
        }
    }
}
