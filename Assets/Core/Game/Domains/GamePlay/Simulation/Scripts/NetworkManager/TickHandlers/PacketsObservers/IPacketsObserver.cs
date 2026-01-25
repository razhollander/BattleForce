using Core.Game.Domains.GamePlay.Shared.C2SModels;
using LiteNetLib;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager.TickHandlers.PacketsObservers
{
    public interface IPacketsObserver
    {
        public PacketTypeC2S PacketType { get; }
        public void OnPacketReceived(NetDataReader reader, NetPeer peer);
    }
}