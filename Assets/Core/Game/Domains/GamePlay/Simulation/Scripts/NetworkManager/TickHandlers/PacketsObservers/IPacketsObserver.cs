using Core.Game.Domains.GamePlay.Shared.C2SModels;
using LiteNetLib;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager
{
    public interface IPacketsParser
    {
        public PacketTypeC2S PacketType { get; }
        public void OnPacketReceived(NetPacketReader reader, NetPeer peer);
    }
}