using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Shared.C2SModels;
using Core.Game.Domains.GamePlay.Shared.ServerToClientModels;
using Core.Scripts.Extensions;
using CoreDomain.Scripts.Services.Logger.Base;
using LiteNetLib;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Presentation.Scripts.Network
{
    public class NetworkS2CPacketsSender
    {
        private readonly NetDataWriter _writer;
        private Dictionary<ushort, NetPeer> _peerPerPlayerId = new();
        private readonly NetPacketProcessor _packetProcessor;

        public NetworkS2CPacketsSender(NetPacketProcessor packetProcessor)
        {
            _writer = new NetDataWriter();
            _packetProcessor = packetProcessor;
        }

        public void AddPlayerPeer(ushort playerId, NetPeer peer)
        {
            if (!_peerPerPlayerId.TryAdd(playerId, peer))
            {
                LogService.LogError($"Peer with player id {playerId} is already added!");
            }
        }
        
        // public void SendPacketSerializedOnlyToPlayer<T>(PacketTypeS2C type, T packet, int playerId, DeliveryMethod deliveryMethod) where T : INetSerializable
        // {
        //     if (_peerPerPlayerId == null)
        //     {
        //         LogService.LogError("NetPeer is null! Must have a peer to send packets to!");
        //         return;
        //     }
        //     
        //     _writer.Reset();
        //     _writer.Put((byte)type);
        //     packet.Serialize(_writer);
        //     LogService.LogTopic($"Send packet {type}, json: {packet.ToJson()}", LogTopicType.ClientNetwork);
        //     _peerPerPlayerId[playerId].Send(_writer, deliveryMethod);
        // }
        
        // public void SendPacketOnlyToPlayer<T>(T packet, DeliveryMethod deliveryMethod, int playerId) where T : class, new()
        // {
        //     if (_peerPerPlayerId == null)
        //     {
        //         LogService.LogError("NetPeer is null! Must have a peer to send packets to!");
        //         return;
        //     }
        //     
        //     _writer.Reset();
        //     _writer.Put((byte) PacketTypeS2C.Serialized);
        //     _packetProcessor.Write(_writer, packet);
        //     _peerPerPlayerId[playerId].Send(_writer, deliveryMethod);
        // }
        
        public void SendPacketToAllPlayersSerialized<T>(PacketTypeS2C type, T packet, DeliveryMethod deliveryMethod) where T : INetSerializable
        {
            if (_peerPerPlayerId == null)
            {
                LogService.LogError("NetPeer is null! Must have a peer to send packets to!");
                return;
            }
            
            /*_writer.Reset();
            _writer.Put((byte)type);
            packet.Serialize(_writer);*/
#if Logs
            LogService.LogTopic($"Send packet type {type}, json: {packet.ToJson()}", LogTopicType.ServerNetwork);
#endif
            _peerPerPlayerId.ForEach(x => _packetProcessor.SendNetSerializable(x.Value, packet, deliveryMethod));
        }
        
        public void SendPacketToPlayerSerialized<T>(ushort playerId, PacketTypeS2C type, T packet, DeliveryMethod deliveryMethod) where T : INetSerializable
        {
            if (_peerPerPlayerId == null || !_peerPerPlayerId.TryGetValue(playerId, out var peer))
            {
                LogService.LogError("NetPeer is null! Must have a peer to send packets to!");
                return;
            }

#if Logs
            LogService.LogTopic($"Send packet type {type} to player {playerId}, json: {packet.ToJson()}", LogTopicType.ServerNetwork);
#endif
            _packetProcessor.SendNetSerializable(peer, packet, deliveryMethod);
        }
        
        // public void SendPacket<T>(T packet, DeliveryMethod deliveryMethod) where T : class, new()
        // {
        //     if (_peerPerPlayerId == null)
        //     {
        //         LogService.LogError("NetPeer is null! Must have a peer to send packets to!");
        //         return;
        //     }
        //     
        //     _writer.Reset();
        //     _writer.Put((byte) PacketType.Serialized);
        //     _packetProcessor.Write(_writer, packet);
        //     _peerPerPlayerId.ForEach(x => x.Value.Send(_writer, deliveryMethod));
        // }
        public int GetPlayerPeerId(ushort playerId)
        {
            return _peerPerPlayerId[playerId].Id;
        }
    }
}