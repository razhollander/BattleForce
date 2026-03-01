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
        private DefaultMatchEnterDataConfig _defaultMatchEnterDataConfig;
        
        private ServerInitiatorEnterData _serverInitiatorEnterData;

        public override void ResolveDependencies()
        {
            _serverNetworkManager = _diContainer.Resolve<IServerNetworkManager>();
            _physicsSimulator = _diContainer.Resolve<IPhysicsSimulator>();
            _simulationStateMachine = _diContainer.Resolve<ISimulationStateMachine>();
            _tickService = _diContainer.Resolve<ITickService>();
            _simulationPersistentData = _diContainer.Resolve<ISimulationPersistentData>();
            _headLessQuitterController = _diContainer.Resolve<IHeadLessQuitterController>();
            _playbackRecorderService = _diContainer.Resolve<IPlaybackRecorderService>();
            _defaultMatchEnterDataConfig = _diContainer.Resolve<DefaultMatchEnterDataConfig>();
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

            if (_playbackRecorderService.IsPlaybackEnabled)
            {
                var matchEnterData = new SimulationMatchEnterData(_playbackRecorderService.Players);
                _simulationStateMachine.ChangeToMatch(matchEnterData);
            }
            else if (_simulationPersistentData.ShouldSkipMatchMaking)
            {
                _simulationStateMachine.ChangeToMatch(_defaultMatchEnterDataConfig.DefaultSimulationMatchEnterData);
            }
            else
            {
                _simulationStateMachine.ChangeToMatchMaking();
            }
            
            _serverNetworkManager.InitEntryPoint(_serverInitiatorEnterData.Port);
            _tickService.UnregisterObserver(this);
        }

        public ServerEntryPointCommand SetEnterData(ServerInitiatorEnterData serverInitiatorEnterData)
        {
            _serverInitiatorEnterData = serverInitiatorEnterData;
            return this;
        }
    }
}