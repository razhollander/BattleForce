using System;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
using Core.Game.Domains.GamePlay.Presentation.Scripts.Network.PacketsHandlers;
using Core.Game.Domains.GamePlay.Shared.C2SModels;
using Core.Game.Domains.GamePlay.Shared.Extensions;
using Core.Scripts.Network;
using CoreDomain.Scripts.Services.Logger.Base;
using LiteNetLib;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Presentation.Scripts.Network
{
    public class NetworkS2CPacketsListener : INetEventListener
    {
        private readonly NetPacketProcessor _packetProcessor;
        
        public int PingToLocalHost { get; private set; }
        public event Action<NetPeer> OnPeerConnected;
        public event Action<NetPeer, DisconnectInfo> OnPeerDisconnected;
        //public event Action<JoinAcceptPacketS2C> OnPlayerJoinedAccepted;
        private readonly CapacityDict<PacketTypeS2C, IPacketsObserver> _packetsObservers;

        public NetworkS2CPacketsListener(NetPacketProcessor packetProcessor, NetworkConfig networkConfig)
        {
            _packetProcessor = packetProcessor;
            RegisterAutoSerializedTypes();
            _packetsObservers = new CapacityDict<PacketTypeS2C, IPacketsObserver>(networkConfig.MaxCap.PacketTypes);
        }
        
        public void RegisterObserver(IPacketsObserver PacketsObserver)
        {
            _packetsObservers.Add(PacketsObserver.PacketType, PacketsObserver);
        }
        
        public void UnregisterObserver(IPacketsObserver PacketsObserver)
        {
            _packetsObservers.Remove(PacketsObserver.PacketType);
        }
        
        private void RegisterAutoSerializedTypes()
        {
            _packetProcessor.RegisterNestedType<Vector2>((w, v) => w.Put(v), r => r.GetVector2());
        }

        // public void SubscribeNetSerializable<T, TUserData>(
        //     Action<T, TUserData> onReceive) where T : INetSerializable, new()
        // {
        //     _packetProcessor.SubscribeNetSerializable(onReceive);
        // }
        
        void INetEventListener.OnNetworkReceive(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod)
        {
            var packetType = (PacketTypeS2C)reader.GetByte();
            _packetsObservers[packetType].OnPacketReceived(reader);
            LogService.LogTopic($"OnNetworkReceive", LogTopicType.ClientNetwork);
            //_packetProcessor.ReadAllPackets(reader);
        }

        void INetEventListener.OnPeerConnected(NetPeer peer)
        {
            LogService.LogTopic("Player connected: " + peer.EndPoint, LogTopicType.ClientNetwork);
            OnPeerConnected?.Invoke(peer);
        }

        void INetEventListener.OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            OnPeerDisconnected?.Invoke(peer, disconnectInfo);
        }

        void INetEventListener.OnNetworkError(IPEndPoint endPoint, SocketError socketError)
        {
            LogService.LogError("NetworkError: " + socketError);
        }

        void INetEventListener.OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader,
            UnconnectedMessageType messageType)
        {
            LogService.LogTopic("OnNetworkReceiveUnconnected", LogTopicType.ClientNetwork);
        }

        void INetEventListener.OnNetworkLatencyUpdate(NetPeer peer, int latency)
        {
            PingToLocalHost = latency;
        }

        void INetEventListener.OnConnectionRequest(ConnectionRequest request)
        {
            LogService.LogTopic("OnConnectionRequest", LogTopicType.ClientNetwork);
            request.Reject();
        }
    }
}
