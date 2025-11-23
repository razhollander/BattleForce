using Core.Game.Domains.GamePlay.Shared.NetworkManager;
using Core.Game.Domains.GamePlay.Simulation.NetworkManager;
using Core.Game.Domains.GamePlay.Simulation.NetworkManager.PacketsHandlers;
using Core.Game.Domains.GamePlay.Simulation.Scripts.MatchModel;
using UnityEngine;
using Zenject;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.ContextInstaller
{
    public class ServerSceneInstaller : MonoInstaller
    {
        [SerializeField] private NetworkConfig _networkConfig;
        
        public override void InstallBindings()
        {
            Container.Bind<IServerInitiator>().To<ServerInitiator>().AsSingle().NonLazy();
            Container.Bind<IServerNetworkManager>().To<ServerNetworkManager>().AsSingle().WithArguments(_networkConfig).NonLazy();
            Container.Bind<IPlayerJoinPacketsHandler>().To<PlayerJoinPacketsHandler>().AsSingle().NonLazy();
            Container.Bind<IMatchDataService>().To<MatchDataService>().AsSingle().WithArguments(_networkConfig).NonLazy();
        }
    }
}
