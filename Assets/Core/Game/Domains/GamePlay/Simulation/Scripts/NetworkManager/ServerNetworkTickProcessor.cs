using System;
using System.Threading;
using Core.Game.Domains.GamePlay.Presentation.Scripts.Network;
using Core.Game.Domains.GamePlay.Shared.NetworkManager;
using Core.Scripts.Network;
using CoreDomain.Scripts.Services.Logger.Base;
using CoreDomain.Scripts.Services.StateMachineService;
using LiteNetLib;

namespace Core.Game.Domains.GamePlay.Simulation.NetworkManager
{
    public class ServerNetworkTickProcessor : ITickProcessor
    {
        public int CurrentTick { get; private set; }

        private readonly NetworkConfig _networkConfig;
        private readonly IServerNetworkManager _networkManager;

        //private readonly IServerPlayersInputListener _serverPlayersInputListener;

        private readonly IStateMachineService _stateMachineService;

        private TimerFixedThreaded _fixedTimer;

        private NetworkStateSimulator _networkStateSimulator;

        public ServerNetworkTickProcessor(NetworkConfig networkConfig, IServerNetworkManager networkManager, IStateMachineService stateMachineService)
        {
            _networkConfig = networkConfig;
            _networkManager = networkManager;
            //_serverPlayersInputListener = serverPlayersInputListener;
        }

        public void InitEntryPoint()
        {
            StartTick();
        }

        private void StartTick()
        {
            var cancellationTokenSource = new CancellationTokenSource();
            _fixedTimer = new TimerFixedThreaded(_networkConfig.TicksPerSeconds, OnTick);
            _fixedTimer.Start(cancellationTokenSource/*_stateMachineService.CurrentState().CancellationTokenSource*/);
        }

        public void InitExitPoint()
        {
            StopTick();
        }

        private void StopTick()
        {
            _fixedTimer.Stop();
        }

        private void OnTick()
        {
            try
            {
                CurrentTick++;
                _networkManager.PollEvents();
                //ProccesEvents();
                //Move1Tick(); // only velocities
                //Simulation.Step();//check collisions
                //ProcessCollisions();
                //SendCurrentTickStateToAllClients();
                //var inputsPerPlayerForCurrentTick = _serverPlayersInputListener.GetSortedInputsPerPlayerForTick(CurrentTick); 
            }
            catch (Exception e)
            {
                LogService.LogError("Got error! " + e.ToString());
                throw;
            }
          
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