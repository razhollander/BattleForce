using CoreDomain.GameDomain.Scripts.GameInitiator;
using CoreDomain.GameDomain.Scripts.States.GamePlayState;
using CoreDomain.Scripts.Services.AudioService;
using UnityEngine;
using Zenject;

namespace CoreDomain.GameDomain.Scripts.ZenjectInstallers
{
    public class GameInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<IGameInitiator>().To<GameInitiator.GameInitiator>().AsSingle().NonLazy();
            Container.BindFactory<GamePlayInitiatorEnterData, GamePlayState, GamePlayState.Factory>();
        }
    }
}