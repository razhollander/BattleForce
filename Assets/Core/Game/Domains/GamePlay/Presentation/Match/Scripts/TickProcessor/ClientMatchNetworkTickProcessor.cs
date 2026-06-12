using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.DataService;
using Core.Game.Domains.GamePlay.Presentation.Scripts.Network;
using Core.Game.Domains.GamePlay.Presentation.Scripts.Network.PacketsHandlers;
using Core.Game.Domains.GamePlay.Presentation.Scripts.Services.DataService;
using Core.Game.Domains.GamePlay.Presentation.Scripts.TickProcessors;
using Core.Game.Domains.GamePlay.Shared;
using CoreDomain.Scripts.Services.CommandFactory;
using CoreDomain.Scripts.Services.StateMachineService;
using CoreDomain.Scripts.Services.UpdateService;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.TickProcessor
{
    public class ClientMatchNetworkTickProcessor : ITickProcessor, IFixedUpdatable
    {
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

        private TimerFixedThreaded2 _fixedTimer;

        public ClientMatchNetworkTickProcessor(IClientNetworkManager networkManager,
            //ClientSimulationStateHandler clientSimulationStateHandler,
            IUpdateSubscriptionService updateSubscriptionService, ICommandFactory commandFactory,
            IFullTickPacketsHandler fullTickPacketsHandler, IMatchDataService matchDataService, ITickCounterService tickCounterService, ILocalPlayersDataService localPlayersDataService)
        {
            _networkManager = networkManager;
            //_clientSimulationStateHandler = clientSimulationStateHandler;
            _updateSubscriptionService = updateSubscriptionService;
            _commandFactory = commandFactory;
            _fullTickPacketsHandler = fullTickPacketsHandler;
            _matchDataService = matchDataService;
            _tickCounterService = tickCounterService;
            _localPlayersDataService = localPlayersDataService;
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
            
            if (_localPlayersDataService.IsClientJoined)
            {
                _sendMatchInputsToServerCommand.Execute();
            }
        }
    }
}