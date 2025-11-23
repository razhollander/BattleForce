using Core.Game.Domains.GamePlay.Shared.NetworkManager;
using Core.Game.Domains.GamePlay.Simulation.NetworkManager.PacketsHandlers;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.ContextInstaller
{
    public class ServerEntryPointCommand : BaseCommand, ICommandVoid
    {
        private IServerNetworkManager _serverNetworkManager;
        private IPlayerJoinPacketsHandler _playerJoinPacketsHandler;

        public override void ResolveDependencies()
        {
            _serverNetworkManager = _diContainer.Resolve<IServerNetworkManager>();
            _playerJoinPacketsHandler = _diContainer.Resolve<IPlayerJoinPacketsHandler>();
        }

        public void Execute()
        {
            _serverNetworkManager.InitEntryPoint();
            _playerJoinPacketsHandler.InitEntryPoint();
        }
    }
}