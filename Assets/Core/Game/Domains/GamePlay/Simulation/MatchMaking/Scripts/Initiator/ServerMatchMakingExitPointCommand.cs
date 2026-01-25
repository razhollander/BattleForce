using Core.Game.Domains.GamePlay.Simulation.MatchMaking.Scripts.NetworkManager.TickHandlers.PacketsObservers;
using Core.Game.Domains.GamePlay.Simulation.MatchMaking.Scripts.TickHandlers.PacketObservers;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Simulation.MatchMaking.Scripts.Initiator
{
    public class ServerMatchMakingExitPointCommand: BaseCommand, ICommandVoid
    {
        private IPlayerJoinPacketsHandler _playerJoinPacketsHandler;
        private ITickProcessor _tickProcessor;
        private IPlayerInputsPacketsHandler _playerInputsPacketsHandler;

        public override void ResolveDependencies()
        {
            _tickProcessor = _diContainer.Resolve<ITickProcessor>();
            _playerJoinPacketsHandler = _diContainer.Resolve<IPlayerJoinPacketsHandler>();
            _playerInputsPacketsHandler = _diContainer.Resolve<IPlayerInputsPacketsHandler>();
        }

        public void Execute()
        {
            _playerJoinPacketsHandler.InitExitPoint();
            _tickProcessor.InitExitPoint();
            _playerInputsPacketsHandler.InitExitPoint();
        }
    }
}