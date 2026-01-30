using Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager.Controllers;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Physics;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Services.SimulationPersistentData;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Services.TickService;
using Core.Game.Domains.GamePlay.Simulation.Scripts.States;
using UnityEngine;
using Zenject;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.ContextInstaller
{
    public class ServerSceneInstaller : MonoInstaller
    {
        [SerializeField] private SimulationGamePlayConfig _gamePlayConfig;
        [SerializeField] private SharedGamePlayConfig _sharedGamePlayConfig;
        [SerializeField] private PowerUpsConfig _powerUpsConfig;

        public override void InstallBindings()
        {
            Container.BindInstance(_sharedGamePlayConfig).AsSingle().NonLazy();
            Container.Bind<ITickService>().To<TickService>().AsSingle().NonLazy();
            Container.Bind<IServerInitiator>().To<ServerInitiator>().AsSingle().NonLazy();
            Container.BindInstance(_gamePlayConfig).AsSingle().NonLazy();
            Container.BindInstance(_powerUpsConfig).AsSingle().NonLazy();
            Container.Bind<IServerNetworkManager>().To<ServerNetworkManager>().AsSingle().NonLazy();
            Container.Bind<INetEventsDataService>().To<NetEventsDataService>().AsSingle().NonLazy();
            Container.Bind<IPhysicsSimulator>().To<PhysicsSimulator>().AsSingle().NonLazy();
            Container.Bind<ISimulationStateMachine>().To<SimulationStateMachine>().AsSingle().NonLazy();
            Container.Bind<ISimulationPersistentData>().To<SimulationPersistentData>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<HeadLessQuitterController>().AsSingle().NonLazy();
        }
    }
}
