using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.NetworkManager.TickHandlers;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.NetworkManager.TickHandlers.PacketsObservers;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Playback;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.PlayersInLavaTracker;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.PowerUpsSpawner;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Talent;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;
using CoreDomain.Scripts.Services.CommandFactory;
using Zenject;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Initiator
{
    public class ServerMatchInstaller // called from reflection
    {
        private readonly DiContainer _diContainer;
        private readonly SimulationMatchEnterData _simulationMatchEnterData;
        private readonly ICommandFactory _commandFactory;

        public ServerMatchInstaller(DiContainer diContainer, SimulationMatchEnterData simulationMatchEnterData) 
        {
            _diContainer = diContainer;
            _simulationMatchEnterData = simulationMatchEnterData;
            _commandFactory = _diContainer.Resolve<ICommandFactory>();
            InstallBindings();
            StartMatch();
        }
        
        private void InstallBindings()
        {
            _diContainer.Bind<IMatchDataService>().To<MatchDataService>().AsSingle();
            _diContainer.Bind<ITickProcessor>().To<ServerMatchNetworkTickProcessor>().AsSingle().NonLazy();
            _diContainer.Bind<IPlayersTalentsManager>().To<PlayersTalentsManager>().AsSingle().NonLazy();
            _diContainer.Bind<IPlayersInLavaTrackerService>().To<PlayersInLavaTrackerService>().AsSingle().NonLazy();
            _diContainer.Bind<IPowerUpsSpawnerService>().To<PowerUpsSpawnTimerService>().AsSingle().NonLazy();
            _diContainer.Bind<IPlayeRejoinPacketsHandler>().To<PlayeRejoinPacketsHandler>().AsSingle().NonLazy();
            _diContainer.Bind<IPlayerInputsPacketsHandler>().To<PlayerInputsPacketsHandler>().AsSingle().NonLazy();
            _diContainer.Bind<IPlaybackRecorderService>().To<PlaybackRecorderService>().AsSingle().NonLazy();
        }

        private void StartMatch()
        {
            _commandFactory.CreateCommandVoid<ServerMatchEntryPointCommand>()
                .SetMatchEnterData(_simulationMatchEnterData)
                .Execute();
        }
    }
}