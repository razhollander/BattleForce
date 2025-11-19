using Core.Game.Domains.GamePlay.Presentation.Features.UI;
using Core.Game.Domains.GamePlay.Shared.NetworkManager;
using CoreDomain.GameDomain.GameStateDomain.GamePlayDomain.Scripts.Initiator;
using CoreDomain.GameDomain.GameStateDomain.GamePlayDomain.Scripts.Mvc.GameInputActions;
using UnityEngine;
using UnityEngine.Rendering;
using Zenject;

namespace CoreDomain.GameDomain.GameStateDomain.GamePlayDomain.Scripts.ZenjectInstallers
{
    public class GamePlayInstaller : MonoInstaller
    {
        [SerializeField] private Volume _postProcessVolume;
        [SerializeField] private ChooseNetworkRoleUIView _chooseNetworkRoleUIView;
        [SerializeField] private NetworkConfig _networkConfig;

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
            Container.Bind<INetworkManager>().To<INetworkManager>().AsSingle().NonLazy();
        }

        private void BindControllers()
        {
            Container.BindInterfacesTo<GameInputActionsController>().AsSingle().NonLazy();
            Container.BindInterfacesTo<ChooseNetworkRoleUIController>().AsSingle().WithArguments(_chooseNetworkRoleUIView).NonLazy();
            Container.BindInterfacesTo<NetworkManager>().AsSingle().WithArguments(_networkConfig).NonLazy();
            // Container.BindInterfacesTo<BFNetworkClient>().AsSingle().NonLazy();
            // Container.BindInterfacesTo<BFNetworkServer>().AsSingle().NonLazy();
            // Container.BindInterfacesTo<BFNetworkManager>().AsSingle().NonLazy();
        }
    }
}