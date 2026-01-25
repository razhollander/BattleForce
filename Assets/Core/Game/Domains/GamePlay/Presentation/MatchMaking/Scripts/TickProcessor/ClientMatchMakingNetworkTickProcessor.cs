using System;
using Core.Game.Domains.GamePlay.Presentation.MatchMaking.Scripts.Commands;
using Core.Game.Domains.GamePlay.Presentation.MatchMaking.Scripts.DataService;
using Core.Game.Domains.GamePlay.Presentation.Scripts.Network;
using Core.Game.Domains.GamePlay.Presentation.Scripts.Network.PacketsHandlers;
using Core.Game.Domains.GamePlay.Presentation.Scripts.TickProcessors;
using Core.Game.Domains.GamePlay.Shared;
using CoreDomain.Scripts.Services.CommandFactory;
using CoreDomain.Scripts.Services.StateMachineService;
using CoreDomain.Scripts.Services.UpdateService;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.MatchMaking.Scripts.TickProcessor
{
    public class ClientMatchMakingNetworkTickProcessor : ITickProcessor, IFixedUpdatable, IGUIUpdatable
    {
        private readonly IUpdateSubscriptionService _updateSubscriptionService;
        private readonly ICommandFactory _commandFactory;
        private readonly IFullTickPacketsHandler _fullTickPacketsHandler;
        private readonly IMatchMakingDataService _matchMakingDataService;
        private readonly ITickCounterService _tickCounterService;
        private readonly IStateMachineService _stateMachineService;
        private readonly IClientNetworkManager _networkManager;

        private SendMatchMakingInputsToServerCommand _sendInputsToServerCommand;
        private TimerFixedThreaded2 _fixedTimer;
        private DateTime _lastSendTime;
        private int _deltaMS;
        private int _highestMs;

        public ClientMatchMakingNetworkTickProcessor(IClientNetworkManager networkManager,
            IUpdateSubscriptionService updateSubscriptionService, ICommandFactory commandFactory,
            IFullTickPacketsHandler fullTickPacketsHandler, IMatchMakingDataService matchMakingDataService, ITickCounterService tickCounterService)
        {
            _networkManager = networkManager;
            _updateSubscriptionService = updateSubscriptionService;
            _commandFactory = commandFactory;
            _fullTickPacketsHandler = fullTickPacketsHandler;
            _matchMakingDataService = matchMakingDataService;
            _tickCounterService = tickCounterService;
        }

        public void InitEntryPoint()
        {
            _sendInputsToServerCommand = _commandFactory.CreateCommandVoid<SendMatchMakingInputsToServerCommand>();
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
            
            if (_matchMakingDataService.IsPlayerJoined)
            {
                SendCurrentTickInputsToServer();
                _deltaMS = DateTime.Now.Millisecond - _lastSendTime.Millisecond;
                _highestMs = Mathf.Max(_deltaMS, _highestMs);
                _lastSendTime = DateTime.Now;
            }
        }

        private void SendCurrentTickInputsToServer()
        {
            _sendInputsToServerCommand.Execute();
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