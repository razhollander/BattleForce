using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.NetworkManager.TickHandlers;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.NetworkManager.TickHandlers.PacketsObservers;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.PlayersInLavaTracker;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.PowerUpsSpawner;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Stage;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Talent;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Inputs;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Services.PlayersForcesService;
using Zenject;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Initiator
{
    public class ServerMatchInstaller
    {
        private  DiContainer _diContainer;

        public ServerMatchInstaller(DiContainer diContainer) 
        {
            _diContainer = diContainer;
        }
        
        public void InstallBindings()
        {
            _diContainer.Bind<IMatchDataService>().To<MatchDataService>().AsSingle();
            _diContainer.Bind<IStageDataService>().To<StageDataService>().AsSingle();
            _diContainer.Bind<ITickProcessor>().To<ServerMatchNetworkTickProcessor>().AsSingle().NonLazy();
            _diContainer.Bind<IPlayersTalentsManager>().To<PlayersTalentsManager>().AsSingle().NonLazy();
            _diContainer.Bind<IPlayersInLavaTrackerService>().To<PlayersInLavaTrackerService>().AsSingle().NonLazy();
            _diContainer.Bind<IPowerUpsSpawnerService>().To<PowerUpsSpawnTimerService>().AsSingle().NonLazy();
            _diContainer.Bind<IMatchPlayerJoinPacketsHandler>().To<MatchPlayerJoinPacketsHandler>().AsSingle().NonLazy();
            _diContainer.Bind<IMatchPlayerInputsPacketsHandler>().To<MatchPlayerInputsPacketsHandler>().AsSingle().NonLazy();
            _diContainer.Bind<IPlayersDecelerationLogic>().To<PlayersDecelerationLogic>().AsSingle().NonLazy();
            _diContainer.Bind<IPlayersEngineLogic>().To<PlayersEngineLogic>().AsSingle().NonLazy();
        }

        public void UninstallBindings() // not sure this is needed
        {
            _diContainer = null;
        }
    }
}