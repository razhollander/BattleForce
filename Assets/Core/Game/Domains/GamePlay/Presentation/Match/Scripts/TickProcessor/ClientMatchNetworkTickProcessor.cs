using System;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.DataService;
using Core.Game.Domains.GamePlay.Presentation.Scripts.Network;
using Core.Game.Domains.GamePlay.Presentation.Scripts.Network.PacketsHandlers;
using Core.Game.Domains.GamePlay.Presentation.Scripts.TickProcessors;
using Core.Game.Domains.GamePlay.Shared;
using CoreDomain.Scripts.Services.CommandFactory;
using CoreDomain.Scripts.Services.StateMachineService;
using CoreDomain.Scripts.Services.UpdateService;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.TickProcessor
{
    public class ClientMatchNetworkTickProcessor : ITickProcessor, IFixedUpdatable, IGUIUpdatable
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

        private TimerFixedThreaded2 _fixedTimer;
        private DateTime _lastSendTime;
        private int _deltaMS;
        private int _highestMs;

        public ClientMatchNetworkTickProcessor(IClientNetworkManager networkManager,
            //ClientSimulationStateHandler clientSimulationStateHandler,
            IUpdateSubscriptionService updateSubscriptionService, ICommandFactory commandFactory,
            IFullTickPacketsHandler fullTickPacketsHandler, IMatchDataService matchDataService, ITickCounterService tickCounterService)
        {
            _networkManager = networkManager;
            //_clientSimulationStateHandler = clientSimulationStateHandler;
            _updateSubscriptionService = updateSubscriptionService;
            _commandFactory = commandFactory;
            _fullTickPacketsHandler = fullTickPacketsHandler;
            _matchDataService = matchDataService;
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
            _updateSubscriptionService.RegisterGuiUpdatable(this);
            _lastSendTime = DateTime.Now;
        }
        
        public void StopTick()
        {
            _updateSubscriptionService.UnregisterFixedUpdatable(this);
            _updateSubscriptionService.UnregisterGuiUpdatable(this);
        }

        public void ManagedFixedUpdate()
        {
            _networkManager.PollEvents();
            _tickCounterService.IncrementTick();
            _fullTickPacketsHandler.ProcessStateLatestTick();
            
            if (_matchDataService.IsPlayerJoined)
            {
                _sendMatchInputsToServerCommand.SetPlayerId(_matchDataService.LocalPlayer.PlayerId).Execute();
                _deltaMS = DateTime.Now.Millisecond - _lastSendTime.Millisecond;
                _highestMs = Mathf.Max(_deltaMS, _highestMs);
                _lastSendTime = DateTime.Now;
            }
        }
        
        public void ManagedOnGUI()
        {
            GUILayout.Label($"delta from last send to server: {_deltaMS} ms, highest: {_highestMs}");
        }

        public void ManagedOnDrawGizmos()
        {
            
        }
    }
}