using System;
using System.Threading;
using CoreDomain.Scripts.Services.Logger.Base;
using CoreDomain.Scripts.Services.StateMachineService;
using CoreDomain.Scripts.Services.UpdateService;
using LiteNetLib;

namespace Core.Game.Domains.GamePlay.Presentation.Scripts.Network
{
    public class ClientNetworkTickProcessor : IFixedUpdatable
    {
        public int CurrentTick;

        private readonly NetManager _netManager;
        private readonly ClientSimulationStateHandler _clientSimulationStateHandler;
        private readonly IUpdateSubscriptionService _updateSubscriptionService;
        private readonly IStateMachineService _stateMachineService;

        private TimerFixedThreaded _fixedTimer;

        public ClientNetworkTickProcessor(NetManager netManager , ClientSimulationStateHandler clientSimulationStateHandler, IUpdateSubscriptionService updateSubscriptionService)
        {
            _netManager = netManager;
            _clientSimulationStateHandler = clientSimulationStateHandler;
            _updateSubscriptionService = updateSubscriptionService;
        }

        public void StartTick(int ticksPerSecond, CancellationTokenSource cancellationTokenSource)
        {
            _updateSubscriptionService.RegisterFixedUpdatable(this);
            // _fixedTimer = new TimerFixedThreaded(ticksPerSecond, OnTick);
            // _fixedTimer.Start(cancellationTokenSource);
        }
        
        public void StopTick()
        {
            _updateSubscriptionService.UnregisterFixedUpdatable(this);
        }

        public void ManagedFixedUpdate()
        {
            CurrentTick++;
            _netManager.PollEvents();
            var SimulationStateForCurrentTick = _clientSimulationStateHandler.GetSortedStates(CurrentTick);
        }

        private void OnTick()
        {
            
            // Pass inputs to Simulator and update Current State
            //_serverState.Tick = CurrentTick;
            // Send current state to all players
            // _playerManager.LogicUpdate();
            // if (_serverTick % 2 == 0)
            // {
            //     
            //     int pCount = _playerManager.Count;
            //     
            //     foreach(ServerPlayer p in _playerManager)
            //     {
            //         SendStateToPlayer(p, pCount);
            //     }
            // }
        }

        // private void SendStateToPlayer(ServerPlayer p, int pCount)

        // {

        //     int statesMax = p.AssociatedPeer.GetMaxSinglePacketSize(DeliveryMethod.Unreliable) - ServerState.HeaderSize;

        //     statesMax /= PlayerState.Size;

        //         

        //     for (int s = 0; s < (pCount-1)/statesMax + 1; s++)

        //     {

        //         //TODO: divide

        //         _serverState.LastProcessedCommand = p.LastProcessedCommandId;

        //         _serverState.PlayerStatesCount = pCount;

        //         _serverState.StartState = s * statesMax;

        //         p.AssociatedPeer.Send(WriteSerializable(PacketType.ServerState, _serverState), DeliveryMethod.Unreliable);

        //     }

        // }


        // private NetDataWriter WriteSerializable<T>(PacketType type, T packet) where T : struct, INetSerializable

        // {

        //     _cachedWriter.Reset();

        //     _cachedWriter.Put((byte) type);

        //     packet.Serialize(_cachedWriter);

        //     return _cachedWriter;

        // }

        //

        // private NetDataWriter WritePacket<T>(T packet) where T : class, new()

        // {

        //     _cachedWriter.Reset();

        //     _cachedWriter.Put((byte) PacketType.Serialized);

        //     _packetProcessor.Write(_cachedWriter, packet);

        //     return _cachedWriter;

        // }
    }
}