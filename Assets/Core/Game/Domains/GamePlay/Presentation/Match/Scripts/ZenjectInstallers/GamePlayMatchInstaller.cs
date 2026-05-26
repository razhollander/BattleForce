using Core.Game.Domains.GamePlay.Presentation.Features.Bullets.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Features.ChickenEggs.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Features.Environment.Background.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Features.Environment.Walls.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Features.Player.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.Bullets.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.DashPulse.Scripts.Effect;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.Environment.FieldBarriers.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.Environment.LavaWalls.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.Environment.Springs.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.Environment.Springs.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.Environment.TeleportGate.Scripts.Mvcs.EnvironmentTeleportGate;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.Environment.TeleportGate.Scripts.Mvcs.PlayerTeleportEffect;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.Environment.Walls.Scripts.Mvcs;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.GainBoltEffect.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.Player.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.PowerUps.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.PowerUps.Scripts.ObtainedEffect;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.SwapFields.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.KOProjectiles.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.GrapplingHook.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.MagneticPullEffect.Scripts;
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
        [SerializeField] private EnvironmentTeleportGateView _environmentTeleportGateViewPrefab;
        [SerializeField] private StageEndedUiView _stageEndedUiView;
        [SerializeField] private TeamsBoardContainerView _teamsBoardContainerView;
        [SerializeField] private GainBoltEffectView gainBoltEffectViewPrefab;
        [SerializeField] private PlayerTeleportEffectView playerTeleportEffectViewPrefab;
        [SerializeField] private EnvironmentFieldBarrierView _environmentFieldBarrierViewPrefab;
        [SerializeField] private SwapFieldView _swapFieldViewPrefab;
        [SerializeField] private KOProjectileView _koProjectileViewPrefab;
        [SerializeField] private GrapplingHookProjectileView _grapplingHookProjectileViewPrefab;
        [SerializeField] private DashPulseGustEffectView _dashPulseGustEffectViewPrefab;
        [SerializeField] private MagneticPullFieldView _magneticPullFieldViewPrefab;
        [SerializeField] private MagneticPullHitEffectView _magneticPullHitEffectViewPrefab;
        [SerializeField] private ChickenEggView _chickenEggViewPrefab;
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
            Container.BindInterfacesTo<Core.Game.Domains.GamePlay.Presentation.Match.Features.ChickenEggs.Scripts.Mvc.MatchChickenEggsControllers>().AsSingle().WithArguments(_chickenEggViewPrefab).NonLazy();
            Container.BindInterfacesTo<MatchEnvironmentWallsControllers>().AsSingle().WithArguments(_environmentWallViewPrefab).NonLazy();
            Container.BindInterfacesTo<EnvironmentSpringControllers>().AsSingle().WithArguments(_environmentSpringViewPrefab).NonLazy();
            Container.BindInterfacesTo<StageEndedUiController>().AsSingle().WithArguments(_stageEndedUiView).NonLazy();
            Container.BindInterfacesTo<TeamsBoardUIController>().AsSingle().WithArguments(_teamsBoardContainerView).NonLazy();
            Container.BindInterfacesTo<GainBoltEffectController>().AsSingle().WithArguments(gainBoltEffectViewPrefab).NonLazy();
            Container.BindInterfacesTo<EnvironmentTeleportGateControllers>().AsSingle().WithArguments(_environmentTeleportGateViewPrefab).NonLazy();
            Container.BindInterfacesTo<PlayerTeleportEffectController>().AsSingle().WithArguments(playerTeleportEffectViewPrefab).NonLazy();
            Container.BindInterfacesTo<EnvironmentFieldBarrierControllers>().AsSingle().WithArguments(_environmentFieldBarrierViewPrefab).NonLazy();
            Container.BindInterfacesTo<KOProjectilesControllers>().AsSingle().WithArguments(_koProjectileViewPrefab).NonLazy();
            Container.BindInterfacesTo<GrapplingHookProjectilesControllers>().AsSingle().WithArguments(_grapplingHookProjectileViewPrefab).NonLazy();
            Container.BindInterfacesTo<DashPulseGustEffectController>().AsSingle().WithArguments(_dashPulseGustEffectViewPrefab).NonLazy();
            Container.BindInterfacesTo<MagneticPullEffectController>().AsSingle().WithArguments(_magneticPullFieldViewPrefab, _magneticPullHitEffectViewPrefab).NonLazy();
            Container.BindInterfacesTo<BackgroundParallaxController>().AsSingle().WithArguments(_backgroundParallaxView).NonLazy();
        }
    }
}
