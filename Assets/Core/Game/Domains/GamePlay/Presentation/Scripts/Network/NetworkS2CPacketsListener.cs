using System;
using System.Net;
using System.Net.Sockets;
using Core.Game.Domains.GamePlay.Shared.C2SModels;
using Core.Game.Domains.GamePlay.Shared.ClientToServerModels;
using Core.Game.Domains.GamePlay.Shared.Extensions;
using Core.Game.Domains.GamePlay.Shared.ServerToClientModels;
using CoreDomain.Scripts.Services.Logger.Base;
using LiteNetLib;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Presentation.Scripts.Network
{
    public class NetworkS2CPacketsListener : INetEventListener
    {
        private NetManager _netManager;
        private NetPacketProcessor _packetProcessor;
        
        public event Action<NetPeer> OnPeerConnected;
        public event Action<NetPeer, DisconnectInfo> OnPeerDisconnected;
        //public event Action<JoinAcceptPacketS2C> OnPlayerJoinedAccepted;

        public NetworkS2CPacketsListener(NetPacketProcessor packetProcessor)
        {
            _packetProcessor = packetProcessor;
            RegisterAutoSerializedTypes();
            _netManager = new NetManager(this) { AutoRecycle = true };
        }
        
        private void RegisterAutoSerializedTypes()
        {
            _packetProcessor.RegisterNestedType((w, v) => w.Put(v), r => r.GetVector2());
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
            _packetProcessor.ReadAllPackets(reader);
            // var packetType = reader.GetByte();
            // var pt = (PacketTypeS2C) packetType;
            // switch (pt)
            // {
            //     case PacketTypeS2C.JoinAccepted:
            //         var playerJoinedAccepted = new JoinAcceptPacketS2C();
            //         playerJoinedAccepted.Deserialize(reader);
            //         OnPlayerJoinedAccepted?.Invoke(playerJoinedAccepted);
            //         break;
            //     default:
            //         LogService.Log("Unhandled packet: " + pt);
            //         break;
            // }
        }
        
        public void PollPackets()
        {
            _netManager.PollEvents();
        }
        
        public void RegisterListeners()
        {
            //register auto serializable PlayerState
            //_packetProcessor.RegisterNestedType<PlayerState>();
            
            //_packetProcessor.SubscribeReusable<JoinAcceptPacket, NetPeer>(OnJoinReceived);
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
            request.Reject();
        }
    }
}
