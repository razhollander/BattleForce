using System;
using Core.Game.Domains.GamePlay.Presentation.MatchMaking.Scripts.Commands;
using Core.Game.Domains.GamePlay.Presentation.MatchMaking.Scripts.DataService;
using Core.Game.Domains.GamePlay.Presentation.MatchMaking.Scripts.Network.PacketsHandlers;
using Core.Game.Domains.GamePlay.Presentation.Scripts.Network;
using Core.Game.Domains.GamePlay.Presentation.Scripts.Network.PacketsHandlers;
using Core.Game.Domains.GamePlay.Presentation.Scripts.TickProcessors;
using Core.Game.Domains.GamePlay.Shared;
using CoreDomain.Scripts.Services.CommandFactory;
using CoreDomain.Scripts.Services.Logger.Base;
using CoreDomain.Scripts.Services.StateMachineService;
using CoreDomain.Scripts.Services.UpdateService;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.MatchMaking.Scripts.TickProcessor
{
    public class ClientMatchMakingNetworkTickProcessor : ITickProcessor, IFixedUpdatable
    {
        private readonly IUpdateSubscriptionService _updateSubscriptionService;
        private readonly ICommandFactory _commandFactory;
        private readonly IFullTickPacketsHandler _fullTickPacketsHandler;
        private readonly IMatchMakingDataService _matchMakingDataService;
        private readonly ITickCounterService _tickCounterService;
        private readonly IStartMatchPacketHandler _startMatchPacketHandler;
        private readonly IStateMachineService _stateMachineService;
        private readonly IClientNetworkManager _networkManager;

        private SendMatchMakingInputsToServerCommand _sendInputsToServerCommand;
        private TimerFixedThreaded2 _fixedTimer;

        public ClientMatchMakingNetworkTickProcessor(IClientNetworkManager networkManager,
            IUpdateSubscriptionService updateSubscriptionService, ICommandFactory commandFactory,
            IFullTickPacketsHandler fullTickPacketsHandler, IMatchMakingDataService matchMakingDataService, ITickCounterService tickCounterService, IStartMatchPacketHandler startMatchPacketHandler)
        {
            _networkManager = networkManager;
            _updateSubscriptionService = updateSubscriptionService;
            _commandFactory = commandFactory;
            _fullTickPacketsHandler = fullTickPacketsHandler;
            _matchMakingDataService = matchMakingDataService;
            _tickCounterService = tickCounterService;
            _startMatchPacketHandler = startMatchPacketHandler;
        }

        public void InitEntryPoint()
        {
            _sendInputsToServerCommand = _commandFactory.CreateCommandVoid<SendMatchMakingInputsToServerCommand>();
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

            SendCurrentTickInputsToServer();

            _startMatchPacketHandler.ProcessStartMatchPacket();
        }

        private void SendCurrentTickInputsToServer()
        {
            _sendInputsToServerCommand.Execute();
        }
    }
}