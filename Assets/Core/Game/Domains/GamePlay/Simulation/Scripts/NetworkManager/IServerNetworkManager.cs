using System;
using Core.Game.Domains.GamePlay.Shared.C2SModels;
using Core.Game.Domains.GamePlay.Shared.ServerToClientModels;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager.TickHandlers.PacketsHandlers;
using LiteNetLib;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.NetworkManager
{
    public interface IServerNetworkManager
    {
        void InitEntryPoint();
        void InitExitPoint();

        // public void SubscribeNetSerializable<T>(
        //     Action<T, int> onReceive) where T : INetSerializable, new();
        // public void SubscribeNetSerializable<T>(
        //     Action<T, NetPeer> onReceive) where T : INetSerializable, new();
        //void SubscribeReusable<T>(Action<T> onReceive) where T : class, new();
      //  void SubscribeReusable<T, TUserData>(Action<T, TUserData> onReceive) where T : class, new();
        // void SendPacket<T>(T packet, DeliveryMethod deliveryMethod) where T : class, new();
        void SendToAllPlayersPacketSerialized<T>(PacketTypeS2C type, T packet, DeliveryMethod deliveryMethod) where T : INetSerializable;

        public void SendPacketToPlayerSerialized<T>(ushort playerId, PacketTypeS2C type, T packet, DeliveryMethod deliveryMethod) where T : INetSerializable;
        // void SendPacketOnlyToPlayer<T>(T packet, DeliveryMethod deliveryMethod, int playerId) where T : class, new();
        //void SendPacketSerializedOnlyToPlayer<T>(PacketTypeS2C type, T packet, int playerId, DeliveryMethod deliveryMethod) where T : INetSerializable;
        //void RemoveSubscription<T>();
        void AddPlayerPeer(ushort playerId, NetPeer peer);
        void PollEvents();
        int GetPlayerPeerId(ushort playerId);
        void RegisterPacketsObserver(IPacketsObserver packetsObserver);
        void UnregisterPacketsObserver(IPacketsObserver packetsObserver);
    }
}