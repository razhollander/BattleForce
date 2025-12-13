using Core.Game.Domains.GamePlay.Presentation.Features.Bullets.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Features.Player.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Features.UI;
using Core.Game.Domains.GamePlay.Presentation.Scripts.GameInputActions;
using Core.Game.Domains.GamePlay.Presentation.Scripts.Initiator;
using Core.Game.Domains.GamePlay.Presentation.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Presentation.Scripts.Network;
using Core.Game.Domains.GamePlay.Presentation.Scripts.Network.PacketsHandlers;
using Core.Game.Domains.GamePlay.Presentation.Scripts.Presentation;
using Core.Game.Domains.GamePlay.Simulation.NetworkManager.PacketsHandlers;
using Core.Scripts.Network;
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

        public override void InstallBindings()
        {
            BindAssets();
            BindServices();
            BindControllers();
        }

        private void BindAssets()
        {
        }

        private void BindServices()
        {
            Container.Bind<IGamePlayInitiator>().To<GamePlayInitiator>().AsSingle().NonLazy();
            Container.Bind<IClientNetworkManager>().To<ClientNetworkManager>().AsSingle().NonLazy();
            Container.Bind<IMatchDataService>().To<MatchDataService>().AsSingle().NonLazy();
            Container.Bind<IMatchNetEventsDataService>().To<MatchNetEventsDataService>().AsSingle().NonLazy();
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
            //Container.BindInterfacesTo<NetworkManager>().AsSingle().WithArguments(_networkConfig).NonLazy();
            // Container.BindInterfacesTo<BFNetworkClient>().AsSingle().NonLazy();
            // Container.BindInterfacesTo<BFNetworkServer>().AsSingle().NonLazy();
            // Container.BindInterfacesTo<BFNetworkManager>().AsSingle().NonLazy();
        }
    }
}