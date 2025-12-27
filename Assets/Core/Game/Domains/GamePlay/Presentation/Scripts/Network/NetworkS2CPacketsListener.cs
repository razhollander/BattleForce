using System;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
using Core.Game.Domains.GamePlay.Presentation.Scripts.Network.PacketsHandlers;
using Core.Game.Domains.GamePlay.Shared.C2SModels;
using Core.Game.Domains.GamePlay.Shared.Extensions;
using CoreDomain.Scripts.Services.Logger.Base;
using LiteNetLib;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Presentation.Scripts.Network
{
    public class NetworkS2CPacketsBroadcaster : INetEventListener
    {
        private readonly NetPacketProcessor _packetProcessor;
        
        public int PingToLocalHost { get; private set; }
        public event Action<NetPeer> OnPeerConnected;
        public event Action<NetPeer, DisconnectInfo> OnPeerDisconnected;
        //public event Action<JoinAcceptPacketS2C> OnPlayerJoinedAccepted;
        private readonly CapacityDict<PacketTypeC2S, IPacketsObserver> _packetsObservers;

        public NetworkS2CPacketsBroadcaster(NetPacketProcessor packetProcessor)
        {
            _packetProcessor = packetProcessor;
            RegisterAutoSerializedTypes();
        }
        
        private void RegisterAutoSerializedTypes()
        {
            _packetProcessor.RegisterNestedType<Vector2>((w, v) => w.Put(v), r => r.GetVector2());
        }

        public void SubscribeNetSerializable<T, TUserData>(
            Action<T, TUserData> onReceive) where T : INetSerializable, new()
        {
            _packetProcessor.SubscribeNetSerializable(onReceive);
        }
        
        public void RemoveSubscription<T>()
        {
            _packetProcessor.RemoveSubscription<T>();
        }
        
        void INetEventListener.OnNetworkReceive(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod)
        {
#if Logs
            LogService.LogTopic($"OnNetworkReceive", LogTopicType.ClientNetwork);
#endif
            _packetProcessor.ReadAllPackets(reader);
        }

        void INetEventListener.OnPeerConnected(NetPeer peer)
        {
#if Logs
            LogService.LogTopic("Player connected: " + peer.EndPoint, LogTopicType.ClientNetwork);
#endif
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
#if Logs
            LogService.LogTopic("OnNetworkReceiveUnconnected", LogTopicType.ClientNetwork);
#endif
        }

        void INetEventListener.OnNetworkLatencyUpdate(NetPeer peer, int latency)
        {
            PingToLocalHost = latency;
        }

        void INetEventListener.OnConnectionRequest(ConnectionRequest request)
        {
#if Logs
            LogService.LogTopic("OnConnectionRequest", LogTopicType.ClientNetwork);
#endif
            request.Reject();
        }
    }
}
