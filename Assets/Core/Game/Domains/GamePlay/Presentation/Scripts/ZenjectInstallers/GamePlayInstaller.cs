using Core.Game.Domains.GamePlay.Presentation.Features.UI;
using Core.Game.Domains.GamePlay.Presentation.Features.UI.ChooseNetworkRole.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Scripts.GameInputActions;
using Core.Game.Domains.GamePlay.Presentation.Scripts.Initiator;
using Core.Game.Domains.GamePlay.Presentation.Scripts.Network;
using Core.Game.Domains.GamePlay.Presentation.Scripts.PresentationEvents;
using Core.Game.Domains.GamePlay.Presentation.Scripts.ScriptableObjects;
using Core.Game.Domains.GamePlay.Presentation.Scripts.TickProcessors;
using UnityEngine;
using Zenject;

namespace Core.Game.Domains.GamePlay.Presentation.Scripts.ZenjectInstallers
{
    public class GamePlayInstaller : MonoInstaller
    {
        [SerializeField] private ChooseNetworkRoleUIView _chooseNetworkRoleUIView;
        [SerializeField] private SharedGamePlayConfig _sharedGamePlayConfig;
        [SerializeField] private PresentationGamePlayConfig _gamePlayConfig;
       

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
            Container.Bind<ICachedPresentationEventsService>().To<CachedPresentationEventsService>().AsSingle().NonLazy();
            Container.Bind<ITickCounterService>().To<TickCounterService>().AsSingle().NonLazy();
        }

        private void BindControllers()
        {
            Container.BindInterfacesTo<GameInputActionsController>().AsSingle().NonLazy();
            Container.BindInterfacesTo<ChooseNetworkRoleUIController>().AsSingle().WithArguments(_chooseNetworkRoleUIView).NonLazy();
        }
    }
}
