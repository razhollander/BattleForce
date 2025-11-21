using Core.Game.Domains.GamePlay.Shared.NetworkManager;
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
            Container.Bind<INetworkManager>().To<NetworkManager>().AsSingle().WithArguments(_networkConfig).NonLazy();
        }
    }
}
