using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Shared.ClientToServerModels;
using LiteNetLib;

namespace Core.Game.Domains.GamePlay.Shared.NetworkManager
{
    public class ServerPlayersInputListener : IServerPlayersInputListener
    {
        private readonly NetworkPacketsListener _networkPacketsListener;
        private List<PlayerInputC2S> _sortedInputs = new List<PlayerInputC2S>();

        public ServerPlayersInputListener(NetworkPacketsListener networkPacketsListener)
        {
            _networkPacketsListener = networkPacketsListener;
        }

        private void OnInputReceived(NetPacketReader reader, NetPeer peer)
        {
            if (peer.Tag == null)
                return;
            _cachedCommand.Deserialize(reader);
            var player = (ServerPlayer) peer.Tag;
            
            bool antilagApplied = _playerManager.EnableAntilag(player);
            player.ApplyInput(_cachedCommand, LogicTimer.FixedDelta);
            if(antilagApplied)
                _playerManager.DisableAntilag();
        }

        public PlayerInputC2S[] GetInputsAndClearCache()
        {
            _sortedInputs.Sort();
            return _sortedInputs.ToArray();
        }
    }

    public interface IServerPlayersInputListener
    {
        PlayerInputC2S[] GetInputsAndClearCache();
    }
}