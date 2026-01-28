using System.Net;
using System.Net.Sockets;
using Core.Game.Domains.GamePlay.Shared.C2SModels;
using Core.Scripts.Network;
using Core.Scripts.Utils.CustomCollections;
using CoreDomain.Scripts.Services.Logger.Base;
using LiteNetLib;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager.TickHandlers.PacketsObservers
{
    public class NetworkC2SPacketsListener : INetEventListener
    {
        private readonly NetworkConfig _networkConfig;
        private readonly IServerNetworkManager _serverNetworkManager;
        private NetManager _netManager;

        private readonly CapacityDict<PacketTypeC2S, IPacketsObserver> _packetsObservers;
        private readonly CapacityList<IRawPacketsObserver> _rawPacketsObservers;

        public NetworkC2SPacketsListener(NetworkConfig networkConfig)
        {
            _networkConfig = networkConfig;
            _packetsObservers = new CapacityDict<PacketTypeC2S, IPacketsObserver>(networkConfig.MaxCap.PacketTypes);
            _rawPacketsObservers = new CapacityList<IRawPacketsObserver>(1);
        }

        public void RegisterObserver(IPacketsObserver PacketsObserver)
        {
            _packetsObservers.Add(PacketsObserver.PacketType, PacketsObserver);
        }
        
        public void UnregisterObserver(IPacketsObserver PacketsObserver)
        {
            _packetsObservers.Remove(PacketsObserver.PacketType);
        }
        
        public void RegisterObserver(IRawPacketsObserver PacketsObserver)
        {
            _rawPacketsObservers.Add(PacketsObserver);
        }
        
        public void UnregisterObserver(IRawPacketsObserver PacketsObserver)
        {
            _rawPacketsObservers.Remove(PacketsObserver);
        }

        public void OnNetworkReceive(NetPeer peer, NetDataReader reader)
        {
            for (int i = _rawPacketsObservers.Count - 1; i >= 0; i--)
            {
                _rawPacketsObservers[i].OnPacketReceived(reader.RawData, peer);
            }
        
            var packetType = (PacketTypeC2S) reader.GetByte();

            if (_packetsObservers.TryGetValue(packetType, out var observer))
            {
                observer.OnPacketReceived(reader, peer);
            }
            LogService.LogTopic($"OnNetworkReceive!", LogTopicType.ServerNetwork);
        }

        void INetEventListener.OnPeerConnected(NetPeer peer)
        {
            LogService.LogTopic("Player connected: " + peer.Address, LogTopicType.ServerNetwork);
        }

        void INetEventListener.OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            // LogService.Log("[S] Player disconnected: " + disconnectInfo.Reason);
            //
            // if (peer.Tag != null)
            // {
            //     byte playerId = (byte)peer.Id;
            //     if (_playerManager.RemovePlayer(playerId))
            //     {
            //         var plp = new PlayerLeavedPacket { Id = (byte)peer.Id };
            //         _netManager.SendToAll(WritePacket(plp), DeliveryMethod.ReliableOrdered);
            //     }
            // }
        }

        void INetEventListener.OnNetworkError(IPEndPoint endPoint, SocketError socketError)
        {
            LogService.LogError("NetworkError: " + socketError);
        }

        public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channelNumber, DeliveryMethod deliveryMethod)
        {
            OnNetworkReceive(peer, reader);
        }

        void INetEventListener.OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader,
            UnconnectedMessageType messageType)
        {
            LogService.LogTopic("OnNetworkReceiveUnconnected! ", LogTopicType.ServerNetwork);
        }

        void INetEventListener.OnNetworkLatencyUpdate(NetPeer peer, int latency)
        {
            // if (peer.Tag != null)
            // {
            //     var p = (ServerPlayer) peer.Tag;
            //     p.Ping = latency;
            // }
        }

        void INetEventListener.OnConnectionRequest(ConnectionRequest request)
        {
            LogService.LogTopic("ConnectionRequest", LogTopicType.ServerNetwork);
            request.AcceptIfKey(_networkConfig.ConntectionKey);
        }
    }

    // public interface INetworkC2SPacketsListener
    // {
    //     void PollPackets();
    //     event Action<PlayerKeyInputsC2S, ushort> PlayerInputReceivedEvent;
    //     event Action<JoinRequestPacketC2S, ushort> PlayerJoinReceivedEvent;
    // }
}
