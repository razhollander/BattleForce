using Core.Game.Domains.GamePlay.Shared.Scripts.MatchInitData;
using Core.Game.Domains.GamePlay.Shared.Scripts.Playback;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Controllers;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Inputs;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Physics;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Playback;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Services.ClientsNetworkDataService;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Services.GamePlayConfig;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Services.SimulationPersistentData;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Services.SimulationSpeedMultiplier;
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
            Container.Bind<ISimulationSpeedMultiplierDataService>().To<SimulationSpeedMultiplierDataService>().AsSingle().NonLazy();
            Container.Bind<ITickService>().To<TickService>().AsSingle().NonLazy();
            Container.Bind<IServerInitiator>().To<ServerInitiator>().AsSingle().NonLazy();
            Container.BindInstance(_powerUpsConfig).AsSingle().NonLazy();
            Container.Bind<IServerNetworkManager>().To<ServerNetworkManager>().AsSingle().NonLazy();
            Container.Bind<INetEventsDataService>().To<NetEventsDataService>().AsSingle().NonLazy();
            Container.Bind<IPhysicsSimulator>().To<PhysicsSimulator>().AsSingle().NonLazy();
            Container.Bind<ISimulationStateMachine>().To<SimulationStateMachine>().AsSingle().NonLazy();
            Container.Bind<ISimulationPersistentData>().To<SimulationPersistentData>().AsSingle().NonLazy();
            Container.Bind<IHeadLessQuitterController>().To<HeadLessQuitterController>().AsSingle().NonLazy();
            Container.Bind<ISimulationSpeedupController>().To<SimulationSpeedupController>().AsSingle().NonLazy();
            Container.Bind<IPlaybackRecorderService>().To<PlaybackRecorderService>().AsSingle().NonLazy();
            Container.Bind<ISimulationGamePlayConfigService>().To<SimulationGamePlayConfigService>().AsSingle().WithArguments(_gamePlayConfig).NonLazy();
            Container.Bind<IPlaybackIOService>().To<PlaybackIOService>().AsSingle().NonLazy();
            Container.Bind<ISimulationInputService>().To<SimulationInputService>().AsSingle();
            Container.Bind<IClientsNetworkDataService>().To<ClientsNetworkDataService>().AsSingle();
        }
    }
}
