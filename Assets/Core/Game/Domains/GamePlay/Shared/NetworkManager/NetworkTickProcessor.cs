using Core.Game.Domains.GamePlay.Shared.ServerToClientModels;
using LiteNetLib;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.NetworkManager
{
    public class NetworkTickProcessor
    {
        public int CurrentTick;
        
        private readonly INetworkPacketsListener _networkPacketsListener;
        private readonly IServerPlayersInputListener _serverPlayersInputListener;
        
        private FixedTimer _fixedTimer;
        private NetworkStateSimulator _networkStateSimulator;

        public NetworkTickProcessor(INetworkPacketsListener networkPacketsListener, IServerPlayersInputListener serverPlayersInputListener)
        {
            _networkPacketsListener = networkPacketsListener;
            _serverPlayersInputListener = serverPlayersInputListener;
        }

        public void StartTick(int ticksPerSecond)
        {
            _fixedTimer = new FixedTimer(ticksPerSecond, OnTick);
            _fixedTimer.Start();
        }
        
        private void OnTick()
        {
            CurrentTick++;
            _networkPacketsListener.PollPackets();
            var orderedUnhandledInputs = _serverPlayersInputListener.GetInputsAndClearCache(); 
            // Get Inputs Per Player
            // Pass inputs to Simulator and update Current State
            // Send current state to all players
            _serverTick = (ushort)((_serverTick + 1) % NetworkGeneral.MaxGameSequence);
            _playerManager.LogicUpdate();
            if (_serverTick % 2 == 0)
            {
                _serverState.Tick = _serverTick;
                _serverState.PlayerStates = _playerManager.PlayerStates;
                int pCount = _playerManager.Count;
                
                foreach(ServerPlayer p in _playerManager)
                {
                    SendStateToPlayer(p, pCount);
                }
            }
        }

        private void SendStateToPlayer(ServerPlayer p, int pCount)
        {
            int statesMax = p.AssociatedPeer.GetMaxSinglePacketSize(DeliveryMethod.Unreliable) - ServerState.HeaderSize;
            statesMax /= PlayerState.Size;
                
            for (int s = 0; s < (pCount-1)/statesMax + 1; s++)
            {
                //TODO: divide
                _serverState.LastProcessedCommand = p.LastProcessedCommandId;
                _serverState.PlayerStatesCount = pCount;
                _serverState.StartState = s * statesMax;
                p.AssociatedPeer.Send(WriteSerializable(PacketType.ServerState, _serverState), DeliveryMethod.Unreliable);
            }
        }

        private void Update()
        {
            _netManager.PollEvents();
            _fixedTimer.Update();
        }
        
        private NetDataWriter WriteSerializable<T>(PacketType type, T packet) where T : struct, INetSerializable
        {
            _cachedWriter.Reset();
            _cachedWriter.Put((byte) type);
            packet.Serialize(_cachedWriter);
            return _cachedWriter;
        }

        private NetDataWriter WritePacket<T>(T packet) where T : class, new()
        {
            _cachedWriter.Reset();
            _cachedWriter.Put((byte) PacketType.Serialized);
            _packetProcessor.Write(_cachedWriter, packet);
            return _cachedWriter;
        }
    }
}