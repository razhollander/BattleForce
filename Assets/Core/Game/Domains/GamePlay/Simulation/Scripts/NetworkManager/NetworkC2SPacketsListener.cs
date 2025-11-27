using System;
using System.Net;
using System.Net.Sockets;
using Core.Game.Domains.GamePlay.Shared.C2SModels;
using Core.Game.Domains.GamePlay.Shared.ClientToServerModels;
using Core.Game.Domains.GamePlay.Shared.Extensions;
using Core.Game.Domains.GamePlay.Shared.ServerToClientModels;
using Core.Scripts.Network;
using CoreDomain.Scripts.Services.Logger.Base;
using LiteNetLib;
using LiteNetLib.Utils;


namespace Core.Game.Domains.GamePlay.Shared.NetworkManager
{
    public class NetworkC2SPacketsListener : INetEventListener
    {
        private readonly NetworkConfig _networkConfig;
        private NetManager _netManager;
        private NetPacketProcessor _packetProcessor;

        // public event Action<PlayerKeyInputsC2S, ushort> PlayerInputReceivedEvent;
        // public event Action<JoinRequestPacketC2S, ushort> PlayerJoinReceivedEvent;

        public NetworkC2SPacketsListener(NetPacketProcessor packetProcessor, NetworkConfig networkConfig)
        {
            _networkConfig = networkConfig;
            _packetProcessor = packetProcessor;
            RegisterAutoSerializedTypes();
        }

        private void RegisterAutoSerializedTypes()
        {
            _packetProcessor.RegisterNestedType((w, v) => w.Put(v), r => r.GetVector2());
        }

        void INetEventListener.OnNetworkReceive(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod)
        {
            LogService.LogTopic("OnNetworkReceive! ", LogTopicType.ServerNetwork);
            _packetProcessor.ReadAllPackets(reader, peer);

            // var packetTypeByte = reader.GetByte();
            // var packetType = (PacketTypeC2S) packetTypeByte;
            // var playerId = (ushort)peer.Tag;
            // switch (packetType)
            // {
            //     case PacketTypeC2S.PlayerInput:
            //         var playerKeyInputs = new PlayerKeyInputsC2S();
            //         playerKeyInputs.Deserialize(reader);
            //         PlayerInputReceivedEvent?.Invoke(playerKeyInputs, playerId);
            //         break;
            //     case PacketTypeC2S.JoinRequest:
            //         var joinRequestPacket = new JoinRequestPacketC2S();
            //         joinRequestPacket.Deserialize(reader);
            //         PlayerJoinReceivedEvent?.Invoke(joinRequestPacket, playerId);
            //         break;
            //     default:
            //         LogService.Log("Unhandled packet: " + packetType);
            //         break;
            // }
        }

        private void RegisterReusablePackets()
        {
            // _packetProcessor.SubscribeReusable<PlayerJoinedPacket>(OnPlayerJoined);
            // _packetProcessor.SubscribeReusable<JoinAcceptPacket>(OnJoinAccept);
            // _packetProcessor.SubscribeReusable<PlayerLeavedPacket>(OnPlayerLeaved);
        }

        // private void OnPlayerJoined(PlayerJoinedPacket packet)

        // {

        //     LogService.Log($"Player joined: {packet.UserName}");

        //     // var remotePlayer = new RemotePlayer(_playerManager, packet.UserName, packet);

        //     // var view = RemotePlayerView.Create(_remotePlayerViewPrefab, remotePlayer);

        //     // _playerManager.AddPlayer(remotePlayer, view);

        // }


        // private void OnJoinAccept(JoinAcceptPacketS2C packetS2C)
        // {
        //     LogService.Log("Join accept. Received player id: " + packetS2C.PlayerId);
        //     // _lastServerTick = packet.ServerTick;
        //     // var clientPlayer = new ClientPlayer(this, _playerManager, _userName, packet.Id);
        //     // var view = ClientPlayerView.Create(_clientPlayerViewPrefab, clientPlayer);
        //     // _playerManager.AddClientPlayer(clientPlayer, view);
        // }
        //
        // private void OnPlayerLeaved(PlayerLeavedPacket packet)
        // {
        //     LogService.Log($"[C] Player leaved");
        //
        //     // var player = _playerManager.RemovePlayer(packet.Id);
        //     // if(player != null)
        //     //     LogService.Log($"[C] Player leaved: {player.Name}");
        // }

        public void RegisterListeners()
        {
            //RegisterReusablePackets();
            
            //register auto serializable PlayerState
            //_packetProcessor.RegisterNestedType<PlayerState>();
            
            //_packetProcessor.SubscribeReusable<JoinPacket, NetPeer>(OnJoinReceived);
        }

        // public void UnregisterListeners()

        // {

        //     UnregisterReusablePackets();

        // }


        // private void UnregisterReusablePackets()

        // {

        //     _packetProcessor.RemoveSubscription<PlayerJoinedPacket>();

        //     //_packetProcessor.RemoveSubscription<JoinAcceptPacket>();

        //     _packetProcessor.RemoveSubscription<PlayerLeavedPacket>();

        // }


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
            LogService.LogTopic("Player connected: " + peer.EndPoint, LogTopicType.ServerNetwork);
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
