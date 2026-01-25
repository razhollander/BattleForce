using Core.Game.Domains.GamePlay.Simulation.MatchMaking.Scripts.NetworkManager;
using Core.Game.Domains.GamePlay.Simulation.MatchMaking.Scripts.NetworkManager.TickHandlers.PacketsObservers;
using Core.Game.Domains.GamePlay.Simulation.MatchMaking.Scripts.TeamFloorTracker;
using Core.Game.Domains.GamePlay.Simulation.MatchMaking.Scripts.TickHandlers.PacketObservers;
using Core.Game.Domains.GamePlay.Simulation.Scripts.MatchMakingModel.MatchMakingModel;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;
using CoreDomain.Scripts.Services.CommandFactory;
using Zenject;

namespace Core.Game.Domains.GamePlay.Simulation.MatchMaking.Scripts.Initiator
{
    public class ServerMatchMakingInstaller // called from reflection
    {
        private readonly DiContainer _diContainer;
        private readonly ICommandFactory _commandFactory;

        public ServerMatchMakingInstaller(DiContainer diContainer) 
        {
            _diContainer = diContainer;
            _commandFactory = _diContainer.Resolve<ICommandFactory>();
            InstallBindings();
            StartMatchMaking();
        }
        
        private void InstallBindings()
        {
            _diContainer.Bind<IMatchMakingDataService>().To<MatchMakingDataService>().AsSingle();
            _diContainer.Bind<ITickProcessor>().To<ServerMatchMakingNetworkTickProcessor>().AsSingle();
            _diContainer.Bind<IPlayerJoinPacketsHandler>().To<MatchMakingPlayerJoinPacketsHandler>().AsSingle().NonLazy();
            _diContainer.Bind<IPlayerInputsPacketsHandler>().To<MatchMakingPlayerInputsPacketsHandler>().AsSingle().NonLazy();
            _diContainer.Bind<IPlayersOnTeamFloorTrackerService>().To<PlayersOnTeamFloorTrackerService>().AsSingle();
        }

        private void StartMatchMaking()
        {
            _commandFactory.CreateCommandVoid<ServerMatchMakingEntryPointCommand>().Execute();
        }
    }
}