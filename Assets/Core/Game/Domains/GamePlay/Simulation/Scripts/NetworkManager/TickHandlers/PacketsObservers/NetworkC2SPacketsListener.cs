using System.Net;
using System.Net.Sockets;
using Core.Game.Domains.GamePlay.Shared.C2SModels;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Playback;
using Core.Scripts.Network;
using Core.Scripts.Utils;
using Core.Scripts.Utils.CustomCollections;
using CoreDomain.Scripts.Services.Logger.Base;
using LiteNetLib;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager
{
    public class NetworkC2SPacketsListener : INetEventListener
    {
        private readonly NetworkConfig _networkConfig;
        private NetManager _netManager;
        private PlaybackService _playbackService;
        private int _currentServerTick; // Updated by ServerNetworkManager/Processor ideally, but here we might need to be passed it?

        private readonly CapacityDict<PacketTypeC2S, IPacketsObserver> _packetsObservers;

        public NetworkC2SPacketsListener(NetworkConfig networkConfig)
        {
            _networkConfig = networkConfig;
            _packetsObservers = new CapacityDict<PacketTypeC2S, IPacketsObserver>(networkConfig.MaxCap.PacketTypes);
        }

        public void SetPlaybackService(PlaybackService playbackService)
        {
            _playbackService = playbackService;
        }

        public void SetCurrentTick(int tick)
        {
            _currentServerTick = tick;
        }

        public void RegisterObserver(IPacketsObserver PacketsObserver)
        {
            _packetsObservers.Add(PacketsObserver.PacketType, PacketsObserver);
        }
        
        public void UnregisterObserver(IPacketsObserver PacketsObserver)
        {
            _packetsObservers.Remove(PacketsObserver.PacketType);
        }
        
        public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channelNumber, DeliveryMethod deliveryMethod)
        {
            // If recording, we need the raw bytes.
            if (!PlaybackSettings.IsPlaybackEnabled && _playbackService != null)
            {
                // To get raw bytes including the Type byte we just read?
                // wait, reader.GetByte() consumes it.
                // We should peek or copy before consuming.
                // However, NetPacketReader wraps a buffer.
                // reader.RawData gives the whole buffer, but reader.UserDataOffset/Size tells us where we are?
                // Actually RawData is the full buffer, but it might contain more than this packet if reused.
                // reader.AvailableBytes is remaining.
                // Best way: get all remaining bytes + the one we are about to read.

                // Since I can't easily peek without potentially modifying internal state or complex logic,
                // I will read the type, then read the rest, and reconstruct the array for storage.

                byte[] rawData = new byte[reader.AvailableBytes];
                reader.GetBytes(rawData, 0, rawData.Length);

                // Record
                ushort playerId = ushort.MaxValue;
                if (peer.Tag != null)
                {
                   playerId = (ushort)peer.Tag;
                }
                _playbackService.RecordPacket(_currentServerTick, playerId, rawData);

                // Reset reader position for processing?
                // No, reader is consumed. I need to CREATE a new reader or REWIND?
                // LiteNetLib Reader doesn't support rewind easily if it's a stream.
                // But it's usually a byte array wrapper.
                // We can't rewind easily.

                // Alternative: Record AFTER reading type? No, I need the Type byte too.

                // Correct approach: Use the data I copied.
                // Reconstruct reader? Or pass the raw data?

                // Better:
                // 1. Get current position (0 usually if fresh packet)
                // 2. Read full data into buffer
                // 3. Record buffer
                // 4. Create NEW reader from buffer for processing

                // Actually, let's look at `OnNetworkReceive`. It's called by LiteNetLib.
                // `reader` is passed.
                // If I consume it, it's gone.

                // So:
                // byte[] fullPacket = new byte[reader.AvailableBytes];
                // reader.GetBytes(fullPacket, 0, fullPacket.Length);

                // _playbackService.Record(..., fullPacket);

                // // Process
                // NetPacketReader newReader = new NetPacketReader(fullPacket);
                // var packetType = (PacketTypeC2S)newReader.GetByte();
                // _packetsObservers[packetType].OnPacketReceived(newReader, peer);

                // BUT wait! OnNetworkReceive implementation:
                // var packetType = (PacketTypeC2S)reader.GetByte();

                // So I will change implementation to:

                byte[] fullPacket = new byte[reader.AvailableBytes];
                reader.GetBytes(fullPacket, 0, fullPacket.Length); // This consumes the reader!

                if (peer.Tag != null)
                {
                     ushort playerId = (ushort)peer.Tag;
                     _playbackService.RecordPacket(_currentServerTick, playerId, fullPacket);
                }

                NetPacketReader newReader = new NetPacketReader(fullPacket);
                var packetType = (PacketTypeC2S)newReader.GetByte();
                 _packetsObservers[packetType].OnPacketReceived(newReader, peer);
            }
            else
            {
                var packetType = (PacketTypeC2S)reader.GetByte();
                _packetsObservers[packetType].OnPacketReceived(reader, peer);
            }

            LogService.LogTopic($"OnNetworkReceive!  {deliveryMethod.ToString()}", LogTopicType.ServerNetwork);
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
