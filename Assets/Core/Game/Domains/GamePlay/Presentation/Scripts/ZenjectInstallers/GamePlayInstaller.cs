using Core.Game.Domains.GamePlay.Both.NetworkManager;
using Core.Game.Domains.GamePlay.Presentation.Features.UI;
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

        public override void InstallBindings()
        {
            BindServices();
            BindControllers();
        }

        private void BindServices()
        {
            Container.Bind<IGamePlayInitiator>().To<GamePlayInitiator>().AsSingle().NonLazy();
        }

        private void BindControllers()
        {
            Container.BindInterfacesTo<GameInputActionsController>().AsSingle().NonLazy();
            Container.BindInterfacesTo<ChooseNetworkRoleUIController>().AsSingle().WithArguments(_chooseNetworkRoleUIView).NonLazy();
            Container.BindInterfacesTo<BFNetworkManager>().AsSingle().NonLazy();
        }
    }
}