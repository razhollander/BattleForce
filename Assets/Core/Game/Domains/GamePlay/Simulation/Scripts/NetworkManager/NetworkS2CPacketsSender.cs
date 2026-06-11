using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Shared.C2SModels;
using Core.Scripts.Extensions;
using CoreDomain.Scripts.Services.Logger.Base;
using LiteNetLib;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager
{
    public class NetworkS2CPacketsSender
    {
        private readonly NetDataWriter _writer;
        private Dictionary<long, NetPeer> _peerPerClientId = new();
        private readonly NetPacketProcessor _packetProcessor;
        
        public NetworkS2CPacketsSender(NetPacketProcessor packetProcessor)
        {
            _writer = new NetDataWriter();
            _packetProcessor = packetProcessor;
        }

        public void AddClientPeer(long clientId, NetPeer peer)
        {
            if (!_peerPerClientId.TryAdd(clientId, peer))
            {
                LogService.LogError($"Peer with player id {clientId} is already added!");
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
        //
        // public void SendPacketToAllPlayersSerialized<T>(PacketTypeS2C type, T packet, DeliveryMethod deliveryMethod) where T : INetSerializable
        // {
        //     if (_peerPerPlayerId == null)
        //     {
        //         LogService.LogError("NetPeer is null! Must have a peer to send packets to!");
        //         return;
        //     }
        //     
        //     /*_writer.Reset();
        //     _writer.Put((byte)type);
        //     packet.Serialize(_writer);*/
        //     LogService.LogTopic($"Send packet type {type}, json: {packet.ToJson()}", LogTopicType.ServerNetwork);
        //     _peerPerPlayerId.ForEach(x => _packetProcessor.SendNetSerializable(x.Value, packet, deliveryMethod));
        // }
        
        public void SendPacketToClientSerialized<T>(long clientId, PacketTypeS2C packetType, T packet, DeliveryMethod deliveryMethod) where T : INetSerializable
        {
            if (_peerPerClientId == null || !_peerPerClientId.TryGetValue(clientId, out var peer))
            {
                LogService.LogError("NetPeer is null! Must have a peer to send packets to!");
                return;
            }
            
            _writer.Reset();
            _writer.Put((byte)packetType);
            packet.Serialize(_writer);
            peer.Send(_writer, deliveryMethod);
            LogService.LogTopic($"Send packet type {packetType} to player {clientId}, json: {packet.ToJson()}", LogTopicType.ServerNetwork);
        }

        public void SendPacketToPeerSerialized<T>(NetPeer peer, PacketTypeS2C packetType, T packet, DeliveryMethod deliveryMethod) where T : INetSerializable
        {
            _writer.Reset();
            _writer.Put((byte)packetType);
            packet.Serialize(_writer);
            peer.Send(_writer, deliveryMethod);
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
        public void RemoveClientPeer(long clientId)
        {
            _peerPerClientId.Remove(clientId);
        }
        
        public bool IsClientConnected(long clientId)
        {
            return _peerPerClientId.ContainsKey(clientId);
        }
        
        public bool TryGetClientPeerId(long clientId, out int peerId)
        {
            if (_peerPerClientId.TryGetValue(clientId, out var peer))
            {
                peerId = peer.Id;
                return true;
            }

            peerId = default;
            return false;
        }
    }
}