using System;
using System.Net;
using System.Net.Sockets;
using Core.Game.Domains.GamePlay.Shared.ClientToServerModels;
using Core.Game.Domains.GamePlay.Shared.Extensions;
using CoreDomain.Scripts.Services.Logger.Base;
using LiteNetLib;
using LiteNetLib.Utils;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Shared.NetworkManager
{
    public class NetworkC2SPacketsListener : INetEventListener, INetworkC2SPacketsListener
    {
        private NetManager _netManager;
        private NetPacketProcessor _packetProcessor;
        
        public event Action<NetPacketReader, NetPeer> InputReceivedEvent;

        public NetworkC2SPacketsListener()
        {
            _packetProcessor = new NetPacketProcessor();
            RegisterAutoSerializedPackets();
            _netManager = new NetManager(this) { AutoRecycle = true };
        }

        private void RegisterAutoSerializedPackets()
        {
            _packetProcessor.RegisterNestedType((w, v) => w.Put(v), r => r.GetVector2());
        }

        public void PollPackets()
        {
            _netManager.PollEvents();
        }
        public void StartListening()
        {
            //_packetProcessor.RegisterNestedType((w, v) => w.Put(v), r => r.GetVector2());
           
            //register auto serializable PlayerState
            //_packetProcessor.RegisterNestedType<PlayerState>();
            
            //_packetProcessor.SubscribeReusable<JoinPacket, NetPeer>(OnJoinReceived);
        }
        
        private void OnDestroy()
        {
            _netManager.Stop();
        }

        // private void OnJoinReceived(JoinPacket joinPacket, NetPeer peer)
        // {
        //     LogService.Log("[S] Join packet received: " + joinPacket.UserName);
        //     var player = new ServerPlayer(_playerManager, joinPacket.UserName, peer);
        //     _playerManager.AddPlayer(player);
        //
        //     player.Spawn(new Vector2(Random.Range(-2f, 2f), Random.Range(-2f, 2f)));
        //
        //     //Send join accept
        //     var ja = new JoinAcceptPacket { Id = player.Id, ServerTick = _serverTick };
        //     peer.Send(WritePacket(ja), DeliveryMethod.ReliableOrdered);
        //
        //     //Send to old players info about new player
        //     var pj = new PlayerJoinedPacket
        //     {
        //         UserName = joinPacket.UserName,
        //         NewPlayer = true,
        //         InitialPlayerState = player.NetworkState,
        //         ServerTick = _serverTick
        //     };
        //     _netManager.SendToAll(WritePacket(pj), DeliveryMethod.ReliableOrdered, peer);
        //
        //     //Send to new player info about old players
        //     pj.NewPlayer = false;
        //     foreach(ServerPlayer otherPlayer in _playerManager)
        //     {
        //         if(otherPlayer == player)
        //             continue;
        //         pj.UserName = otherPlayer.Name;
        //         pj.InitialPlayerState = otherPlayer.NetworkState;
        //         peer.Send(WritePacket(pj), DeliveryMethod.ReliableOrdered);
        //     }
        // }

       

        // public void SendShoot(ref ShootPacket sp)
        // {
        //     _netManager.SendToAll(WriteSerializable(PacketType.Shoot, sp), DeliveryMethod.ReliableUnordered);
        // }

        void INetEventListener.OnPeerConnected(NetPeer peer)
        {
            LogService.Log("[S] Player connected: " + peer.EndPoint);
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
            LogService.Log("[S] NetworkError: " + socketError);
        }

        void INetEventListener.OnNetworkReceive(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod)
        {
            byte packetType = reader.GetByte();
            PacketTypeC2S pt = (PacketTypeC2S) packetType;
            switch (pt)
            {
                case PacketTypeC2S.PlayerInput:
                    InputReceivedEvent?.Invoke(reader, peer);
                    break;
                default:
                    LogService.Log("Unhandled packet: " + pt);
                    break;
            }
        }

        void INetEventListener.OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader,
            UnconnectedMessageType messageType)
        {

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
            request.AcceptIfKey("ExampleGame");
        }
    }

    public interface INetworkC2SPacketsListener
    {
        void PollPackets();
        event Action<NetPacketReader, NetPeer> InputReceivedEvent;
    }
}
