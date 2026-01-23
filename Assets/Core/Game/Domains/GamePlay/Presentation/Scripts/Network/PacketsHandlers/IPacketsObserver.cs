using Core.Game.Domains.GamePlay.Shared.C2SModels;
using LiteNetLib;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Presentation.Scripts.Network.PacketsHandlers
{
    public interface IPacketsObserver
    {
        public PacketTypeS2C PacketType { get; }
        public void OnPacketReceived(NetDataReader reader);
    }
}