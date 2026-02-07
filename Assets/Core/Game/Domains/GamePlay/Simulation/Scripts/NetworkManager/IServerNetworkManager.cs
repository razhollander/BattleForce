using System;
using Core.Game.Domains.GamePlay.Shared.C2SModels;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager.TickHandlers.PacketsObservers;
using LiteNetLib;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager
{
    public interface IServerNetworkManager
    {
        int ConnectedPeersCount { get; }
        event Action OnPacketReceivedEvent;
        event Action OnPeerDisconnectedEvent;
        void InitEntryPoint();
        void InitExitPoint();
        //void SendToAllPlayersPacketSerialized<T>(PacketTypeS2C type, T packet, DeliveryMethod deliveryMethod) where T : INetSerializable;
        public void SendPacketToPlayerSerialized<T>(ushort playerId, PacketTypeS2C type, T packet, DeliveryMethod deliveryMethod) where T : INetSerializable;
        public void SendPacketToPeerSerialized<T>(NetPeer peer, PacketTypeS2C type, T packet, DeliveryMethod deliveryMethod) where T : INetSerializable;
        void AddPlayerPeer(ushort playerId, NetPeer peer);
        void PollEvents();
        int GetPlayerPeerId(ushort playerId);
        void RegisterPacketsObserver(IPacketsObserver packetsObserver);
        void RegisterPacketsObserver(IRawPacketsObserver packetsObserver);
        void UnregisterPacketsObserver(IPacketsObserver packetsObserver);
        void UnregisterPacketsObserver(IRawPacketsObserver packetsObserver);
        void SwitchToNetManager(INetManagerWrapper netManagerWrapper);
    }
}