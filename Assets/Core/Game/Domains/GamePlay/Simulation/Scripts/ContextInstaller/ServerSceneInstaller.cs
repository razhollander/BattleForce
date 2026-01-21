using Core.Game.Domains.GamePlay.Shared.NetworkManager;
using Core.Game.Domains.GamePlay.Simulation.NetworkManager;
using Core.Game.Domains.GamePlay.Simulation.NetworkManager.PacketsHandlers;
using Core.Game.Domains.GamePlay.Simulation.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager.Configurations;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager.TickHandlers;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager.TickHandlers.PacketsHandlers;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager.TickHandlers.PacketsObservers.PacketsHandlers;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager.Configurations;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager.TickHandlers;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Physics;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Talent;
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
            Container.Bind<IServerInitiator>().To<ServerInitiator>().AsSingle().NonLazy();
            Container.BindInstance(_gamePlayConfig).AsSingle().NonLazy();
            Container.BindInstance(_powerUpsConfig).AsSingle().NonLazy();
            Container.Bind<IServerNetworkManager>().To<ServerNetworkManager>().AsSingle().NonLazy();
            Container.Bind<IPlayerJoinPacketsHandler>().To<PlayerJoinPacketsHandler>().AsSingle().NonLazy();
            Container.Bind<IPlayerInputsPacketsHandler>().To<PlayerInputsPacketsHandler>().AsSingle().NonLazy();
            Container.Bind<IMatchDataService>().To<MatchDataService>().AsSingle().NonLazy();
            Container.Bind<IMatchNetEventsDataService>().To<MatchNetEventsDataService>().AsSingle().NonLazy();
            Container.Bind<ITickProcessor>().To<ServerNetworkTickProcessor>().AsSingle().NonLazy();
            //Container.Bind<IPlayerBulletsTransformHandler>().To<PlayerBulletsTransformHandler>().AsSingle().NonLazy();
            //Container.Bind<IPowerUpBallsTransformHandler>().To<PowerUpBallsTransformHandler>().AsSingle().NonLazy();
            // Container.Bind<IPlayersTransformHandler>().To<PlayersTransformHandler>().AsSingle().NonLazy();
            Container.Bind<IPhysicsSimulator>().To<PhysicsSimulator>().AsSingle().NonLazy();
            Container.Bind<IPlayersTalentsManager>().To<PlayersTalentsManager>().AsSingle().NonLazy();
            Container.Bind<IPlayersInLavaTrackerService>().To<PlayersInLavaTrackerService>().AsSingle().NonLazy();
            Container.BindInterfacesTo<PowerUpsSpawnerController>().AsSingle().NonLazy();
        }
    }
}
