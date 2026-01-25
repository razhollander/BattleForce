using System;
using System.Collections.Concurrent;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Shared.S2CModels.PacketEvents;

namespace Core.Game.Domains.GamePlay.Shared.NetworkManager
{
    public static class LocalPacketBridge
    {
        // Queue for Server -> Client packets
        private static ConcurrentQueue<MatchFullTickPacket> _serverToClientPackets = new ConcurrentQueue<MatchFullTickPacket>();

        public static void SendToClient(MatchFullTickPacket packet)
        {
            _serverToClientPackets.Enqueue(packet);
        }

        public static bool TryGetNextPacket(out MatchFullTickPacket packet)
        {
            return _serverToClientPackets.TryDequeue(out packet);
        }

        public static void Clear()
        {
            while (_serverToClientPackets.TryDequeue(out _)) { }
        }
    }
}
