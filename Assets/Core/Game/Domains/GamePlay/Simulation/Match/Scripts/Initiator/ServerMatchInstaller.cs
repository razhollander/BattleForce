using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.FrigidBlock;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.NetworkManager.TickHandlers;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.NetworkManager.TickHandlers.PacketsObservers;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.OverrideableNetEvents;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.PlayersInLavaTracker;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.PlayersOutsideStageTracker;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.PlayersTouchingSpikesTracker;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.PlayersTouchingWall;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MolesSpawner;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.PowerUpsSpawner;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.PlayerLockOnTarget;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.PowerUp;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Services.PlayersForcesService;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Stage;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Talent;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Services.TeleportGate;
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
            _diContainer.Bind<IPreparationPhaseTimerService>().To<PreparationPhaseTimerService>().AsSingle();
            _diContainer.Bind<ITickProcessor>().To<ServerMatchNetworkTickProcessor>().AsSingle().NonLazy();
            _diContainer.Bind<IPlayersTalentsManager>().To<PlayersTalentsManager>().AsSingle().NonLazy();
            _diContainer.Bind<IFrigidBlocksController>().To<FrigidBlocksController>().AsSingle().NonLazy();
            _diContainer.Bind<IPlayersPowerUpsManager>().To<PlayersPowerUpsManager>().AsSingle().NonLazy();
            _diContainer.Bind<IPlayersInLavaTrackerService>().To<PlayersInLavaTrackerService>().AsSingle().NonLazy();
            _diContainer.Bind<IPlayersOutsideStageTrackerService>().To<PlayersOutsideStageTrackerService>().AsSingle().NonLazy();
            _diContainer.Bind<IPlayersTouchingWallDataService>().To<PlayersTouchingWallDataService>().AsSingle().NonLazy();
            _diContainer.Bind<IPlayersTouchingSpikesTrackerService>().To<PlayersTouchingSpikesTrackerService>().AsSingle().NonLazy();
            _diContainer.Bind<IOverrideableNetEventsService>().To<OverrideableNetEventsService>().AsSingle().NonLazy();
            _diContainer.Bind<IPowerUpsSpawnerService>().To<PowerUpsSpawnTimerService>().AsSingle().NonLazy();
            _diContainer.Bind<IMolesSpawnerService>().To<MolesSpawnTimerService>().AsSingle().NonLazy();
            _diContainer.Bind<IMatchPlayerJoinPacketsHandler>().To<MatchPlayerJoinPacketsHandler>().AsSingle().NonLazy();
            _diContainer.Bind<IMatchPlayerInputsPacketsHandler>().To<MatchPlayerInputsPacketsHandler>().AsSingle().NonLazy();
            _diContainer.Bind<IPlayersDecelerationLogic>().To<PlayersDecelerationLogic>().AsSingle().NonLazy();
            _diContainer.Bind<IPlayersEngineLogic>().To<PlayersEngineLogic>().AsSingle().NonLazy();
            _diContainer.Bind<ILockOnTargetTimerService>().To<LockOnTargetTimerService>().AsSingle().NonLazy();
            _diContainer.Bind<ITeleportGateService>().To<TeleportGateCooldownService>().AsSingle();
            _diContainer.Bind<IMatchEnvironmentConfigDataService>().To<MatchEnvironmentConfigDataService>().AsSingle();
        }

        public void UninstallBindings() // not sure this is needed
        {
            _diContainer = null;
        }
    }
}