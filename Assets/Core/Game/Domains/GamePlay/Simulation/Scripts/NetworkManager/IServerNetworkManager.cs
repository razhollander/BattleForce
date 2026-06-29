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
        event Action OnClientPeerConnectedEvent;
        event Action<long> OnClientPeerDisconnectedEvent;
        void InitEntryPoint(int port);
        void InitExitPoint();
        //void SendToAllPlayersPacketSerialized<T>(PacketTypeS2C type, T packet, DeliveryMethod deliveryMethod) where T : INetSerializable;
        public void SendPacketToClientSerialized<T>(long clientId, PacketTypeS2C type, T packet, DeliveryMethod deliveryMethod) where T : INetSerializable;
        public void SendPacketToPeerSerialized<T>(NetPeer peer, PacketTypeS2C type, T packet, DeliveryMethod deliveryMethod) where T : INetSerializable;
        void AddClientPeer(long clientId, NetPeer peer);
        void RemoveClientPeer(long clientId);
        void PollEvents();
        bool TryGetClientPeerId(long clientId, out int peerId);
        void RegisterPacketsObserver(IPacketsObserver packetsObserver);
        void RegisterPacketsObserver(IRawPacketsObserver packetsObserver);
        void UnregisterPacketsObserver(IPacketsObserver packetsObserver);
        void UnregisterPacketsObserver(IRawPacketsObserver packetsObserver);
        void SwitchToNetManager(INetManagerWrapper netManagerWrapper);
        bool IsClientPeerConencted(long clientId);
    }
}