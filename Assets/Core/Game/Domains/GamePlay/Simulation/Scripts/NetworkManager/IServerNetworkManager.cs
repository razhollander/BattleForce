using Core.Game.Domains.GamePlay.Shared.C2SModels;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;
using LiteNetLib;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.NetworkManager
{
    public interface IServerNetworkManager
    {
        void InitEntryPoint();
        void InitExitPoint();
        //void SendToAllPlayersPacketSerialized<T>(PacketTypeS2C type, T packet, DeliveryMethod deliveryMethod) where T : INetSerializable;
        public void SendPacketToPlayerSerialized<T>(ushort playerId, PacketTypeS2C type, T packet, DeliveryMethod deliveryMethod) where T : INetSerializable;
        void AddPlayerPeer(ushort playerId, NetPeer peer);
        void PollEvents();
        int GetPlayerPeerId(ushort playerId);
        void RegisterPacketsObserver(IPacketsObserver packetsObserver);
        void UnregisterPacketsObserver(IPacketsObserver packetsObserver);
        void SwitchToPlayback();
    }
}