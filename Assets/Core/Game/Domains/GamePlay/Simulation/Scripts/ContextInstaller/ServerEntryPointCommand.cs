using Core.Game.Domains.GamePlay.Shared.NetworkManager;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.ContextInstaller
{
    public class ServerEntryPointCommand : BaseCommand, ICommandVoid
    {
        private INetworkManager _networkManager;
        private INetworkTickProcessor _networkTickProcessor;

        public override void ResolveDependencies()
        {
            _networkManager = _diContainer.Resolve<INetworkManager>();
            _networkTickProcessor = _diContainer.Resolve<INetworkTickProcessor>();
        }

        public void Execute()
        {
            _networkManager.InitEntryPoint();
        }
    }
}