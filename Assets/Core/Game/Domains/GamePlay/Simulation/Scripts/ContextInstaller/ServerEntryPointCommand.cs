using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Shared.Scripts.MatchInitData;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Controllers;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Physics;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Playback;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Services.SimulationPersistentData;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Services.TickService;
using Core.Game.Domains.GamePlay.Simulation.Scripts.States;
using Core.Scripts.Utils;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.ContextInstaller
{
    public class ServerEntryPointCommand : BaseCommand, ICommandVoid, ITickObserver
    {
        private IServerNetworkManager _serverNetworkManager;
        private IPhysicsSimulator _physicsSimulator;
        private ISimulationStateMachine _simulationStateMachine;
        private ITickService _tickService;
        private ISimulationPersistentData _simulationPersistentData;
        private IHeadLessQuitterController _headLessQuitterController;
        private IPlaybackRecorderService _playbackRecorderService;
        private SharedGamePlayConfig _sharedGamePlayConfig;
        
        private ServerInitiatorEnterData _serverInitiatorEnterData;

        public ServerEntryPointCommand SetEnterData(ServerInitiatorEnterData serverInitiatorEnterData)
        {
            _serverInitiatorEnterData = serverInitiatorEnterData;
            return this;
        }

        public override void ResolveDependencies()
        {
            _serverNetworkManager = _diContainer.Resolve<IServerNetworkManager>();
            _physicsSimulator = _diContainer.Resolve<IPhysicsSimulator>();
            _simulationStateMachine = _diContainer.Resolve<ISimulationStateMachine>();
            _tickService = _diContainer.Resolve<ITickService>();
            _simulationPersistentData = _diContainer.Resolve<ISimulationPersistentData>();
            _headLessQuitterController = _diContainer.Resolve<IHeadLessQuitterController>();
            _playbackRecorderService = _diContainer.Resolve<IPlaybackRecorderService>();
            _sharedGamePlayConfig = _diContainer.Resolve<SharedGamePlayConfig>();
        }

        public void Execute()
        { 
            _simulationPersistentData.InitEntryPoint();
            _tickService.StartTick();
            _tickService.RegisterObserver(this);
        }

        public void OnTick(int currentTick)
        {
            Init();
        }

        private void Init()
        {
            _playbackRecorderService.InitEntryPoint(_serverInitiatorEnterData.IsPlaybackEnabled, _serverInitiatorEnterData.PlaybackFileName);
            _physicsSimulator.InitEntryPoint();
            _simulationStateMachine.InitEntryPoint();
            _headLessQuitterController.InitEntryPoint();

            var clientId = _simulationPersistentData.DeviceUniqueIdentifier;
            if (_playbackRecorderService.IsPlaybackEnabled)
            {
                var playersPerClientId = new Dictionary<long, EnterMatchPlayerData[]> {{clientId, _playbackRecorderService.Players}};
                var matchEnterData = new SimulationMatchEnterData(playersPerClientId);
                _simulationStateMachine.ChangeToMatch(matchEnterData);
            }
            else if (_simulationPersistentData.ShouldSkipMatchMaking)
            {
                var playersPerClientId = new Dictionary<long, EnterMatchPlayerData[]> {{clientId, _sharedGamePlayConfig.DefaultMatchEnterDataConfig.Players}};
                var matchEnterData = new SimulationMatchEnterData(playersPerClientId);
                _simulationStateMachine.ChangeToMatch(matchEnterData);
            }
            else
            {
                _simulationStateMachine.ChangeToMatchMaking();
            }
            
            _serverNetworkManager.InitEntryPoint(_serverInitiatorEnterData.Port);
            _tickService.UnregisterObserver(this);
        }
    }
}