using Core.Game.Domains.GamePlay.Simulation.MatchMaking.Scripts.NetworkManager;
using Core.Game.Domains.GamePlay.Simulation.MatchMaking.Scripts.NetworkManager.TickHandlers.PacketsObservers;
using Core.Game.Domains.GamePlay.Simulation.MatchMaking.Scripts.StartMatchWall;
using Core.Game.Domains.GamePlay.Simulation.MatchMaking.Scripts.TeamFloorTracker;
using Core.Game.Domains.GamePlay.Simulation.MatchMaking.Scripts.TickHandlers.PacketObservers;
using Core.Game.Domains.GamePlay.Simulation.Scripts.MatchMakingModel.MatchMakingModel;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;
using CoreDomain.Scripts.Services.CommandFactory;
using Zenject;

namespace Core.Game.Domains.GamePlay.Simulation.MatchMaking.Scripts.Initiator
{
    public class ServerMatchMakingInstaller 
    {
        private DiContainer _diContainer;

        public ServerMatchMakingInstaller(DiContainer diContainer) 
        {
            _diContainer = diContainer;
        }
        
        public void InstallBindings()
        {
            _diContainer.Bind<IMatchMakingDataService>().To<MatchMakingDataService>().AsSingle().NonLazy();
            _diContainer.Bind<ITickProcessor>().To<ServerMatchMakingNetworkTickProcessor>().AsSingle().NonLazy();
            _diContainer.Bind<IPlayerJoinPacketsHandler>().To<MatchMakingPlayerJoinPacketsHandler>().AsSingle().NonLazy();
            _diContainer.Bind<IPlayerInputsPacketsHandler>().To<MatchMakingPlayerInputsPacketsHandler>().AsSingle().NonLazy();
            _diContainer.Bind<IPlayersOnTeamFloorTrackerService>().To<PlayersOnTeamFloorTrackerService>().AsSingle().NonLazy();
            _diContainer.Bind<IStartMatchWallController>().To<StartMatchWallController>().AsSingle().NonLazy();
        }

        public void UninstallBindings()
        {
            _diContainer = null;
        }
    }
}