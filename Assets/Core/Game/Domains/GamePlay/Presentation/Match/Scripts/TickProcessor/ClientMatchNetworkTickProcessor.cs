using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.DataService;
using Core.Game.Domains.GamePlay.Presentation.Scripts.Network;
using Core.Game.Domains.GamePlay.Presentation.Scripts.Network.PacketsHandlers;
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
        private readonly Core.Game.Domains.GamePlay.Presentation.Scripts.DataService.ILocalPlayersDataService _localPlayersDataService;
        private readonly ITickCounterService _tickCounterService;
        private readonly IStateMachineService _stateMachineService;
        private SendMatchInputsToServerCommand _sendMatchInputsToServerCommand;
        private readonly IClientNetworkManager _networkManager;

        private TimerFixedThreaded2 _fixedTimer;

        public ClientMatchNetworkTickProcessor(IClientNetworkManager networkManager,
            //ClientSimulationStateHandler clientSimulationStateHandler,
            IUpdateSubscriptionService updateSubscriptionService, ICommandFactory commandFactory,
            IFullTickPacketsHandler fullTickPacketsHandler, IMatchDataService matchDataService, Core.Game.Domains.GamePlay.Presentation.Scripts.DataService.ILocalPlayersDataService localPlayersDataService, ITickCounterService tickCounterService)
        {
            _networkManager = networkManager;
            //_clientSimulationStateHandler = clientSimulationStateHandler;
            _updateSubscriptionService = updateSubscriptionService;
            _commandFactory = commandFactory;
            _fullTickPacketsHandler = fullTickPacketsHandler;
            _matchDataService = matchDataService;
            _localPlayersDataService = localPlayersDataService;
            _tickCounterService = tickCounterService;
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
            
            if (_matchDataService.IsPlayerJoined)
            {
                foreach (var localPlayerId in _localPlayersDataService.LocalPlayerIds)
                {
                    _sendMatchInputsToServerCommand.SetPlayerId(localPlayerId).Execute();
                }
            }
        }
    }
}