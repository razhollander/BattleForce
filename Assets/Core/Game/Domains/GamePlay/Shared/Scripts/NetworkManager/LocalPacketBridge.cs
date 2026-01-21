using System;
using System.Collections.Concurrent;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Shared.S2CModels.PacketEvents;

namespace Core.Game.Domains.GamePlay.Shared.NetworkManager
{
    public static class LocalPacketBridge
    {
        // Queue for Server -> Client packets
        private static ConcurrentQueue<FullTickPacket> _serverToClientPackets = new ConcurrentQueue<FullTickPacket>();

        public static void SendToClient(FullTickPacket packet)
        {
            _serverToClientPackets.Enqueue(packet);
        }

        public static bool TryGetNextPacket(out FullTickPacket packet)
        {
            return _serverToClientPackets.TryDequeue(out packet);
        }

        public static void Clear()
        {
            while (_serverToClientPackets.TryDequeue(out _)) { }
        }
    }
}
