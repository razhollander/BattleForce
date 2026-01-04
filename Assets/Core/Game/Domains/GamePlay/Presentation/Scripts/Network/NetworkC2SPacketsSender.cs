using Core.Game.Domains.GamePlay.Shared.C2SModels;
using Core.Game.Domains.GamePlay.Shared.ServerToClientModels;
using Core.Scripts.Extensions;
using CoreDomain.Scripts.Services.Logger.Base;
using LiteNetLib;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Presentation.Scripts.Network
{
    public class NetworkC2SPacketsSender
    {
        private NetPeer _peer;
        private NetDataWriter _writer = new NetDataWriter();

        public NetworkC2SPacketsSender()
        {
        }

        public NetPeer Peer => _peer;

        public void SetPeer(NetPeer peer)
        {
            _peer = peer;
        }
        
        public void SendPacketSerialized<T>(PacketTypeC2S packetType, T packet, DeliveryMethod deliveryMethod) where T : INetSerializable
        {
            if (_peer == null)
            {
                LogService.LogError("NetPeer is null! Must have a peer to send packets to!");
                return;
            }
            
            _writer.Reset();
            _writer.Put((byte)packetType);
            packet.Serialize(_writer);
            _peer.Send(_writer, deliveryMethod);
            LogService.LogTopic($"Send packet type: {packetType}, json {packet.ToJson()}", LogTopicType.ClientNetwork);
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