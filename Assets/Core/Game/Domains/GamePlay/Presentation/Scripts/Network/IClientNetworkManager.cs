using System;
using Core.Game.Domains.GamePlay.Shared.C2SModels;
using Core.Game.Domains.GamePlay.Shared.ServerToClientModels;
using LiteNetLib;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Presentation.Scripts.Network
{
    public interface IClientNetworkManager
    {
        bool IsPeerConnected { get; }
        int Ping { get; }
        void StartClient();
        void InitExitPoint();


        //void SubscribeReusable<T>(Action<T> onReceive) where T : class, new();

        //void SubscribeReusable<T, TUserData>(Action<T, TUserData> onReceive) where T : class, new();

        public void SubscribeNetSerializable<T, TUserData>(
            Action<T, TUserData> onReceive) where T : INetSerializable, new();

        void SendPacketSerialized<T>(PacketTypeC2S type, T packet, DeliveryMethod deliveryMethod) where T : INetSerializable;
        void RemoveSubscription<T>();
        void PollEvents();
    }
}