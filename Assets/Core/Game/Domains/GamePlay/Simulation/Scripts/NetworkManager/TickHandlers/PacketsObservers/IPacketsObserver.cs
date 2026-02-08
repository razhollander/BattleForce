using Core.Game.Domains.GamePlay.Shared.C2SModels;
using LiteNetLib;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager.TickHandlers.PacketsObservers
{
    public interface IPacketsObserver
    {
        PacketTypeC2S PacketType { get; }
        void OnPacketReceived(NetDataReader reader, NetPeer peer, bool isReceivedFromPlayback);
    }
}