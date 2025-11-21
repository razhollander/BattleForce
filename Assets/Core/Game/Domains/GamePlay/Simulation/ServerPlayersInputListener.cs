using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Shared.ClientToServerModels;
using Core.Game.Domains.GamePlay.Shared.NetworkManager;
using CoreDomain.Scripts.Services.Logger.Base;
using LiteNetLib;

namespace Core.Game.Domains.GamePlay.Simulation
{
    public class ServerPlayersInputListener : IServerPlayersInputListener
    {
        private readonly NetworkC2SPacketsListener _networkC2SPacketsListener;
        private readonly Dictionary<ushort, List<PlayerKeyInputsC2S>> _inputsByPlayer = new ();

        public ServerPlayersInputListener(NetworkC2SPacketsListener networkC2SPacketsListener)
        {
            _networkC2SPacketsListener = networkC2SPacketsListener;
        }

        public void InitEntryPoint()
        {
            _networkC2SPacketsListener.InputReceivedEvent += OnInputReceived;
        }

        private void OnInputReceived(NetPacketReader reader, NetPeer peer)
        {
            var packetType = (PacketTypeC2S)reader.GetByte();
            switch (packetType)
            {
                case PacketTypeC2S.PlayerInput:
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

        public Dictionary<ushort, List<PlayerKeyInputsC2S>> GetSortedInputsPerPlayerForTick(int tick)
        {
            foreach (var kvp in _inputsByPlayer)
            {
                kvp.Value.Sort();
            }
            
            return _inputsByPlayer;
        }

        public void InitExitPoint()
        {
            _networkC2SPacketsListener.InputReceivedEvent -= OnInputReceived;
        }
    }

    public interface IServerPlayersInputListener
    {
        void InitEntryPoint();
        void InitExitPoint();
        Dictionary<ushort, List<PlayerKeyInputsC2S>> GetSortedInputsPerPlayerForTick(int tick);
    }
}