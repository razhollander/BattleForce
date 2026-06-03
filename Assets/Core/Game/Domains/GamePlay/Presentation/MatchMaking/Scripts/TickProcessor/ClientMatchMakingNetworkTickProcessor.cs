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
using UnityEngine.InputSystem;
using Core.Game.Domains.GamePlay.Shared.C2SModels.Packets;
using LiteNetLib;
using Core.Game.Domains.GamePlay.Shared.C2SModels;

namespace Core.Game.Domains.GamePlay.Presentation.MatchMaking.Scripts.TickProcessor
{
    public class ClientMatchMakingNetworkTickProcessor : ITickProcessor, IFixedUpdatable
    {
        private readonly IUpdateSubscriptionService _updateSubscriptionService;
        private readonly ICommandFactory _commandFactory;
        private readonly IFullTickPacketsHandler _fullTickPacketsHandler;
        private readonly IMatchMakingDataService _matchMakingDataService;
        private readonly Core.Game.Domains.GamePlay.Presentation.Scripts.DataService.ILocalPlayersDataService _localPlayersDataService;
        private readonly ITickCounterService _tickCounterService;
        private readonly IStartMatchPacketHandler _startMatchPacketHandler;
        private readonly IStateMachineService _stateMachineService;
        private readonly IClientNetworkManager _networkManager;

        private SendMatchMakingInputsToServerCommand _sendInputsToServerCommand;
        private TimerFixedThreaded2 _fixedTimer;

        public ClientMatchMakingNetworkTickProcessor(IClientNetworkManager networkManager,
            IUpdateSubscriptionService updateSubscriptionService, ICommandFactory commandFactory,
            IFullTickPacketsHandler fullTickPacketsHandler, IMatchMakingDataService matchMakingDataService, Core.Game.Domains.GamePlay.Presentation.Scripts.DataService.ILocalPlayersDataService localPlayersDataService, ITickCounterService tickCounterService, IStartMatchPacketHandler startMatchPacketHandler)
        {
            _networkManager = networkManager;
            _updateSubscriptionService = updateSubscriptionService;
            _commandFactory = commandFactory;
            _fullTickPacketsHandler = fullTickPacketsHandler;
            _matchMakingDataService = matchMakingDataService;
            _localPlayersDataService = localPlayersDataService;
            _tickCounterService = tickCounterService;
            _startMatchPacketHandler = startMatchPacketHandler;
        }

        public void InitEntryPoint()
        {
            _sendInputsToServerCommand = _commandFactory.CreateCommandVoid<SendMatchMakingInputsToServerCommand>();
            StartTick();
            InputSystem.onDeviceChange += OnDeviceChange;
        }

        private void OnDeviceChange(InputDevice device, InputDeviceChange change)
        {
            if (change == InputDeviceChange.Added && device is Gamepad gamepad)
            {
                // Check if device is already assigned to a player
                bool isDeviceAssigned = false;
                foreach (var playerId in _localPlayersDataService.LocalPlayerIds)
                {
                    if (_localPlayersDataService.GetInputDeviceForPlayer(playerId) == device)
                    {
                        isDeviceAssigned = true;
                        break;
                    }
                }

                if (!isDeviceAssigned)
                {
                    var joinRequest = new JoinRequestPacketC2S("Player " + device.deviceId, true);
                    _networkManager.SendPacketSerialized(PacketTypeC2S.JoinRequest, joinRequest, DeliveryMethod.ReliableOrdered);
                }
            }
        }

        private void StartTick()
        {
            _updateSubscriptionService.RegisterFixedUpdatable(this);
        }
        
        public void StopTick()
        {
            _updateSubscriptionService.UnregisterFixedUpdatable(this);
            InputSystem.onDeviceChange -= OnDeviceChange;
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
            foreach (var localPlayerId in _localPlayersDataService.LocalPlayerIds)
            {
                _sendInputsToServerCommand.SetPlayerId(localPlayerId).Execute();
            }
        }
    }
}