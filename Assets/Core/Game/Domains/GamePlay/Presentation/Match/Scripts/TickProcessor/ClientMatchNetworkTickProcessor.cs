using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.DataService;
using Core.Game.Domains.GamePlay.Presentation.Scripts.Network;
using Core.Game.Domains.GamePlay.Presentation.Scripts.Network.PacketsHandlers;
using Core.Game.Domains.GamePlay.Presentation.Scripts.Services.DataService;
using Core.Game.Domains.GamePlay.Presentation.Scripts.DataService;
using Core.Game.Domains.GamePlay.Presentation.Scripts.TickProcessors;
using Core.Game.Domains.GamePlay.Shared;
using CoreDomain.Scripts.Services.CommandFactory;
using CoreDomain.Scripts.Services.StateMachineService;
using CoreDomain.Scripts.Services.UpdateService;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.TickProcessor
{
    public class ClientMatchNetworkTickProcessor : ITickProcessor, IFixedUpdatable
    {
        private const int NO_PEER_PING_IN_MILLISECONDS = 0;

        //private readonly ClientSimulationStateHandler _clientSimulationStateHandler;
        private readonly IUpdateSubscriptionService _updateSubscriptionService;
        private readonly ICommandFactory _commandFactory;
        private readonly IFullTickPacketsHandler _fullTickPacketsHandler;
        private readonly IMatchDataService _matchDataService;
        private readonly ITickCounterService _tickCounterService;
        private readonly IStateMachineService _stateMachineService;
        private SendMatchInputsToServerCommand _sendMatchInputsToServerCommand;
        private readonly IClientNetworkManager _networkManager;
        private readonly ILocalPlayersDataService _localPlayersDataService;
        private readonly INetworkDiagnosticsService _networkDiagnosticsService;

        private TimerFixedThreaded2 _fixedTimer;

        public ClientMatchNetworkTickProcessor(IClientNetworkManager networkManager,
            //ClientSimulationStateHandler clientSimulationStateHandler,
            IUpdateSubscriptionService updateSubscriptionService, ICommandFactory commandFactory,
            IFullTickPacketsHandler fullTickPacketsHandler, IMatchDataService matchDataService, ITickCounterService tickCounterService, ILocalPlayersDataService localPlayersDataService,
            INetworkDiagnosticsService networkDiagnosticsService)
        {
            _networkManager = networkManager;
            //_clientSimulationStateHandler = clientSimulationStateHandler;
            _updateSubscriptionService = updateSubscriptionService;
            _commandFactory = commandFactory;
            _fullTickPacketsHandler = fullTickPacketsHandler;
            _matchDataService = matchDataService;
            _tickCounterService = tickCounterService;
            _localPlayersDataService = localPlayersDataService;
            _networkDiagnosticsService = networkDiagnosticsService;
        }

        public void InitEntryPoint()
        {
            _sendMatchInputsToServerCommand = _commandFactory.CreateCommandVoid<SendMatchInputsToServerCommand>();
            StartTick();
        }

        private void StartTick()
        {
            _updateSubscriptionService.RegisterFixedUpdatable(this);
        }
        
        public void StopTick()
        {
            _updateSubscriptionService.UnregisterFixedUpdatable(this);
        }

        public void ManagedFixedUpdate()
        {
            _networkManager.PollEvents();
            _tickCounterService.IncrementTick();
            _fullTickPacketsHandler.ProcessStateLatestTick();
            // After the poll drained, so this tick's packet count is final. Ping is only read while a peer exists -
            // diagnostics must never be the thing that throws.
            var pingInMilliseconds = _networkManager.IsPeerConnected ? _networkManager.Ping : NO_PEER_PING_IN_MILLISECONDS;
            _networkDiagnosticsService.OnPollCompleted(pingInMilliseconds);

            if (_localPlayersDataService.IsClientJoined)
            {
                _sendMatchInputsToServerCommand.Execute();
            }
        }
    }
}