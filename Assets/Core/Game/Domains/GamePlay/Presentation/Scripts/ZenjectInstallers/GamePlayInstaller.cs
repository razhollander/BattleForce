using Core.Game.Domains.GamePlay.Presentation.Features.Bullets.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Features.Environment.LavaWalls.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Features.Environment.Walls;
using Core.Game.Domains.GamePlay.Presentation.Features.Player.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Features.Player.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Features.TalentCards.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Features.UI;
using Core.Game.Domains.GamePlay.Presentation.Scripts.GameInputActions;
using Core.Game.Domains.GamePlay.Presentation.Scripts.Initiator;
using Core.Game.Domains.GamePlay.Presentation.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Presentation.Scripts.Network;
using Core.Game.Domains.GamePlay.Presentation.Scripts.Network.PacketsHandlers;
using Core.Game.Domains.GamePlay.Presentation.Scripts.Presentation;
using Core.Game.Domains.GamePlay.Presentation.Scripts.ScriptableObjects;
using UnityEngine;
using UnityEngine.Rendering;
using Zenject;

namespace Core.Game.Domains.GamePlay.Presentation.Scripts.ZenjectInstallers
{
    public class GamePlayInstaller : MonoInstaller
    {
        [SerializeField] private Volume _postProcessVolume;
        [SerializeField] private ChooseNetworkRoleUIView _chooseNetworkRoleUIView;
        [SerializeField] private PlayerView _playerViewPrefab;
        [SerializeField] private BulletView _bulletViewPrefab;
        [SerializeField] private EnvironmentWallView _environmentWallViewPrefab;
        [SerializeField] private EnvironmentLavaWallView _environmentLavaWallViewPrefab;
        [SerializeField] private TalentCardView _talentCardViewPrefab;
        [SerializeField] private SharedGamePlayConfig _sharedGamePlayConfig;
        [SerializeField] private PresentationGamePlayConfig _gamePlayConfig;
        [SerializeField] private TalentCardObtainedEffectView _talentCardObtainedEffectView;

        public override void InstallBindings()
        {
            BindAssets();
            BindServices();
            BindControllers();
        }

        private void BindAssets()
        {
            Container.BindInstance(_sharedGamePlayConfig).AsSingle().NonLazy();
            Container.BindInstance(_gamePlayConfig).AsSingle().NonLazy();
        }

        private void BindServices()
        {
            Container.Bind<IGamePlayInitiator>().To<GamePlayInitiator>().AsSingle().NonLazy();
            Container.Bind<IClientNetworkManager>().To<ClientNetworkManager>().AsSingle().NonLazy();
            Container.Bind<IMatchDataService>().To<MatchDataService>().AsSingle().NonLazy();
            Container.Bind<ICachedPresentationEventsService>().To<CachedPresentationEventsService>().AsSingle().NonLazy();
            Container.Bind<ITickProcessor>().To<ClientNetworkTickProcessor>().AsSingle().NonLazy();
            Container.Bind<IFullTickPacketsHandler>().To<FullTickPacketsHandler>().AsSingle().NonLazy();
            Container.Bind<IClientPresentationTickProcessor>().To<ClientPresentationTickProcessor>().AsSingle().NonLazy();
            //Container.Bind<INetworkManager>().To<INetworkManager>().AsSingle().NonLazy();
        }

        private void BindControllers()
        {
            Container.BindInterfacesTo<GameInputActionsController>().AsSingle().NonLazy();
            Container.BindInterfacesTo<ChooseNetworkRoleUIController>().AsSingle().WithArguments(_chooseNetworkRoleUIView).NonLazy();
            Container.BindInterfacesTo<PlayerControllers>().AsSingle().WithArguments(_playerViewPrefab).NonLazy();
            Container.BindInterfacesTo<BulletControllers>().AsSingle().WithArguments(_bulletViewPrefab).NonLazy();
            Container.BindInterfacesTo<EnvironmentWallsControllers>().AsSingle().WithArguments(_environmentWallViewPrefab).NonLazy();
            Container.BindInterfacesTo<EnvironmentLavaWallsControllers>().AsSingle().WithArguments(_environmentLavaWallViewPrefab).NonLazy();
            Container.BindInterfacesTo<TalentCardControllers>().AsSingle().WithArguments(_talentCardViewPrefab).NonLazy();
            Container.BindInterfacesTo<TalentCardObtainedEffectController>().AsSingle().WithArguments(_talentCardObtainedEffectView).NonLazy();
            //Container.BindInterfacesTo<NetworkManager>().AsSingle().WithArguments(_networkConfig).NonLazy();
            // Container.BindInterfacesTo<BFNetworkClient>().AsSingle().NonLazy();
            // Container.BindInterfacesTo<BFNetworkServer>().AsSingle().NonLazy();
            // Container.BindInterfacesTo<BFNetworkManager>().AsSingle().NonLazy();
        }
    }
}