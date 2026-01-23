using System.Net;
using System.Net.Sockets;
using Core.Game.Domains.GamePlay.Shared.C2SModels;
using Core.Game.Domains.GamePlay.Shared.NetworkManager;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Playback;
using Core.Scripts.Network;
using Core.Scripts.Utils;
using Core.Scripts.Utils.CustomCollections;
using CoreDomain.Scripts.Services.Logger.Base;
using LiteNetLib;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager
{
    public class NetworkC2SPacketsListener : INetEventListener
    {
        private readonly NetworkConfig _networkConfig;
        private readonly IPlaybackRecorderService _playbackRecorderService;
        private readonly IServerNetworkManager _serverNetworkManager;
        private NetManager _netManager;

        private readonly CapacityDict<PacketTypeC2S, IPacketsObserver> _packetsObservers;

        public NetworkC2SPacketsListener(NetworkConfig networkConfig, IPlaybackRecorderService playbackRecorderService)
        {
            _networkConfig = networkConfig;
            _playbackRecorderService = playbackRecorderService;
            _packetsObservers = new CapacityDict<PacketTypeC2S, IPacketsObserver>(networkConfig.MaxCap.PacketTypes);
        }

        public void RegisterObserver(IPacketsObserver PacketsObserver)
        {
            _packetsObservers.Add(PacketsObserver.PacketType, PacketsObserver);
        }
        
        public void UnregisterObserver(IPacketsObserver PacketsObserver)
        {
            _packetsObservers.Remove(PacketsObserver.PacketType);
        }

        public void OnNetworkReceive(NetPeer peer, NetDataReader reader)
        {
            if (!_playbackRecorderService.IsPlaybackEnabled && peer.Tag!=null) // todo make this pretty
            {
                var playerId = (ushort)peer.Tag;
                _playbackRecorderService.RecordPacket(playerId, reader.RawData);
            }
            var packetType = (PacketTypeC2S) reader.GetByte();
            _packetsObservers[packetType].OnPacketReceived(reader, peer);
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
