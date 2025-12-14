using Core.Game.Domains.GamePlay.Shared.C2SModels;
using Core.Game.Domains.GamePlay.Shared.ServerToClientModels;
using CoreDomain.Scripts.Extensions;
using CoreDomain.Scripts.Services.Logger.Base;
using LiteNetLib;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Presentation.Scripts.Network
{
    public class NetworkC2SPacketsSender
    {
        private NetPeer _peer;
        private readonly NetPacketProcessor _packetProcessor;

        public NetworkC2SPacketsSender(NetPacketProcessor packetProcessor)
        {
            _packetProcessor = packetProcessor;
        }

        public NetPeer Peer => _peer;

        public void SetPeer(NetPeer peer)
        {
            _peer = peer;
        }
        
        public void SendPacketSerialized<T>(PacketTypeC2S type, T packet, DeliveryMethod deliveryMethod) where T : INetSerializable
        {
            if (_peer == null)
            {
                LogService.LogError("NetPeer is null! Must have a peer to send packets to!");
                return;
            }
            
            // _writer.Reset();
            // _writer.Put((byte)type);
            //packet.Serialize(_writer);
            LogService.LogTopic($"Send packet type: {type}, json {packet.ToJson()}", LogTopicType.ClientNetwork);
            _packetProcessor.SendNetSerializable(_peer, packet, deliveryMethod);
            // _peer.Send(_writer, deliveryMethod);
        }
        
        // public void SendPacket<T>(T packet, DeliveryMethod deliveryMethod) where T : class, new()
        // {
        //     if (_peer == null)
        //     {
        //         LogService.LogError("NetPeer is null! Must have a peer to send packets to!");
        //         return;
        //     }
        //     
        //     _writer.Reset();
        //     _writer.Put((byte) PacketType.Serialized);
        //     _packetProcessor.Write(_writer, packet);
        //     _peer.Send(_writer, deliveryMethod);
        // }
    }
}