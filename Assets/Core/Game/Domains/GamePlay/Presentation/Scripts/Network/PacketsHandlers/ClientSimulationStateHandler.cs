using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Shared.C2SModels;
using Core.Game.Domains.GamePlay.Shared.ClientToServerModels;
using CoreDomain.Scripts.Services.Logger.Base;
using LiteNetLib;

namespace Core.Game.Domains.GamePlay.Presentation.Scripts.Network
{
    public class ClientSimulationStateHandler
    {
        private readonly NetworkS2CPacketsListener _networkS2CPacketsListener;
        private readonly Dictionary<ushort, List<PlayerKeyInputsC2S>> _inputsByPlayer = new ();

        public ClientSimulationStateHandler(NetworkS2CPacketsListener networkS2CPacketsListener)
        {
            _networkS2CPacketsListener = networkS2CPacketsListener;
        }

        public void InitEntryPoint()
        {
            //_networkS2CPacketsListener.OnPlayerJoinedAccepted += OnInputReceived;
        }

        private void OnInputReceived(NetPacketReader reader, NetPeer peer)
        {
            var packetType = (PacketTypeS2C)reader.GetByte();
            switch (packetType)
            {
                case PacketTypeS2C.JoinAccepted:
                {
                    HandlePlayerInput(reader, peer);
                    break;
                }
                case PacketTypeS2C.SimulationState:
                {
                    HandlePlayerInput(reader, peer);
                    break;
                }
                default: LogService.LogError($"Packet type not supported {packetType}"); break;
            }
        }

        private void HandlePlayerInput(NetPacketReader reader, NetPeer peer)
        {
            var input = new PlayerKeyInputsC2S();
            input.Deserialize(reader);
            var playerId = (ushort)peer.Tag;
            _inputsByPlayer.TryAdd(playerId, new List<PlayerKeyInputsC2S>());
            _inputsByPlayer[playerId].Add(input);
        }

        public Dictionary<ushort, List<PlayerKeyInputsC2S>> GetSortedStates(int tick)
        {
            foreach (var kvp in _inputsByPlayer)
            {
                kvp.Value.Sort();
            }
            
            return _inputsByPlayer;
        }

        public void InitExitPoint()
        {
           // _networkS2CPacketsListener.InputReceivedEvent -= OnInputReceived;
        }
    }
}