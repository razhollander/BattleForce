using System;
using Core.Game.Domains.GamePlay.Shared.C2SModels;
using Core.Game.Domains.GamePlay.Shared.ServerToClientModels;
using LiteNetLib;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.NetworkManager
{
    public interface IServerNetworkManager
    {
        void InitEntryPoint();
        void InitExitPoint();

        public void SubscribeNetSerializable<T, TUserData>(
            Action<T, TUserData> onReceive) where T : INetSerializable, new();
        //void SubscribeReusable<T>(Action<T> onReceive) where T : class, new();
      //  void SubscribeReusable<T, TUserData>(Action<T, TUserData> onReceive) where T : class, new();
        // void SendPacket<T>(T packet, DeliveryMethod deliveryMethod) where T : class, new();
        void SendPacketSerialized<T>(PacketTypeS2C type, T packet, DeliveryMethod deliveryMethod) where T : INetSerializable;
        // void SendPacketOnlyToPlayer<T>(T packet, DeliveryMethod deliveryMethod, int playerId) where T : class, new();
        void SendPacketSerializedOnlyToPlayer<T>(PacketTypeS2C type, T packet, int playerId, DeliveryMethod deliveryMethod) where T : INetSerializable;
        void RemoveSubscription<T>();
        void AddPlayerPeer(int playerId, NetPeer peer);
    }
}