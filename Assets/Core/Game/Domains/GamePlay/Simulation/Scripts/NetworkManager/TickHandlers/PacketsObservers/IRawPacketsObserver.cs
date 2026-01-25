using LiteNetLib;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager.TickHandlers.PacketsObservers
{
    public interface IRawPacketsObserver
    {
        void OnPacketReceived(byte[] packetBytes, NetPeer peer);
    }
}