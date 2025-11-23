using System.Threading;
using Core.Game.Domains.GamePlay.Shared.NetworkManager;
using CoreDomain.Scripts.Services.StateMachineService;

namespace Core.Game.Domains.GamePlay.Simulation.NetworkManager
{
    public class SeverNetworkTickProcessor
    {
        public int CurrentTick;
        
        private readonly NetworkC2SPacketsListener _networkC2SPacketsListener;
        private readonly IServerPlayersInputListener _serverPlayersInputListener;
        private readonly IStateMachineService _stateMachineService;

        private FixedTimer _fixedTimer;
        private NetworkStateSimulator _networkStateSimulator;

        public SeverNetworkTickProcessor(NetworkC2SPacketsListener networkC2SPacketsListener, IServerPlayersInputListener serverPlayersInputListener)
        {
            _networkC2SPacketsListener = networkC2SPacketsListener;
            _serverPlayersInputListener = serverPlayersInputListener;
        }

        public void StartTick(int ticksPerSecond, CancellationTokenSource cancellationTokenSource)
        {
            _fixedTimer = new FixedTimer(ticksPerSecond, OnTick);
            _fixedTimer.Start(cancellationTokenSource);
        }
        
        public void StopTick()
        {
            _fixedTimer.Stop();
        }
        
        private void OnTick()
        {
            CurrentTick++;
            _networkC2SPacketsListener.PollPackets();
            var inputsPerPlayerForCurrentTick = _serverPlayersInputListener.GetSortedInputsPerPlayerForTick(CurrentTick); 
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