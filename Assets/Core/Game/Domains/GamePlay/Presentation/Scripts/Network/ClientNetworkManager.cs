using System;
using Core.Game.Domains.GamePlay.Presentation.Scripts.Network.PacketsHandlers;
using Core.Game.Domains.GamePlay.Shared.C2SModels;
using Core.Game.Domains.GamePlay.Shared.NetworkManager;
using Core.Scripts.Network;
using Core.Scripts.Utils;
using CoreDomain.Scripts.Services.CommandFactory;
using CoreDomain.Scripts.Services.Logger.Base;
using CoreDomain.Scripts.Services.UpdateService;
using LiteNetLib;
using LiteNetLib.Utils;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Scripts.Network
{
    public class ClientNetworkManager : IClientNetworkManager, IGUIUpdatable
    {
        private readonly NetworkS2CPacketsListener _packetsListener;
        private readonly NetManager _netManager;
        private readonly NetworkConfig _networkConfig;
        private readonly ICommandFactory _commandFactory;
        private readonly IUpdateSubscriptionService _updateSubscriptionService;
        private readonly NetworkC2SPacketsSender _packetsSender;
        private readonly GUIStyle _guiStyle;
        public bool IsPeerConnected { get; private set; }
        public int Ping => _packetsSender.Peer.Ping;
        public int LocalPeerId => _packetsSender.Peer.Id;

        public ClientNetworkManager(NetworkConfig networkConfig, ICommandFactory commandFactory, IUpdateSubscriptionService updateSubscriptionService)
        {
            _networkConfig = networkConfig;
            _commandFactory = commandFactory;
            _updateSubscriptionService = updateSubscriptionService;
            _packetsListener = new NetworkS2CPacketsListener(networkConfig);
            _packetsSender = new NetworkC2SPacketsSender();
            _netManager = new NetManager(_packetsListener)
            {
                AutoRecycle = true,
                IPv6Enabled = false
            };
            
            _guiStyle = new GUIStyle();
            _guiStyle.fontSize = 10;
            _guiStyle.normal.textColor = Color.white;
        }

        public void StartClient(bool isHost)
        {
            if (_netManager.IsRunning)
            {
                LogService.LogError("Client already running!");
                return;
            }

            if (PlaybackSettings.IsPlaybackEnabled)
            {
                // In Playback, we simulate connection for the logic to proceed (UI hiding etc),
                // but we don't start LiteNetLib.
                _updateSubscriptionService.RegisterGuiUpdatable(this);
                // Fake successful connection
                _commandFactory.CreateCommandVoid<HandleClientConnectedToPeerCommand>().Execute();
                IsPeerConnected = true;
                LogService.LogTopic("Client started in Playback Mode (Network Disabled)", LogTopicType.ClientNetwork);
                return;
            }
            
            _packetsListener.OnPeerConnected += OnServerPeerReceived;
            _updateSubscriptionService.RegisterGuiUpdatable(this);
            _netManager.Start();
            //_packetsListener.RegisterListeners();
            var peerToServer = _netManager.Connect(isHost ?"localhost" :_networkConfig.IpAddress, _networkConfig.HostPort, _networkConfig.ConntectionKey);
            _packetsSender.SetPeer(peerToServer);
           // bool canReachServer = CanPing(_networkConfig.IpAddress);
            //Console.WriteLine("Can reach server: " + canReachServer);
        }
        
        // public static bool CanPing(string address)
        // {
        //     try
        //     {
        //         Ping ping = new Ping();
        //         PingReply reply = ping.Send(address);
        //         return (reply.Status == IPStatus.Success);
        //     }
        //     catch (Exception)
        //     {
        //         return false;
        //     }
        // }

        private void OnServerPeerReceived(NetPeer peerToServer)
        {
            LogService.LogTopic("Server peer received!", LogTopicType.ClientNetwork);
            _packetsSender.SetPeer(peerToServer);
            _commandFactory.CreateCommandVoid<HandleClientConnectedToPeerCommand>().Execute();
            IsPeerConnected = true;
        }

        // public void SubscribeReusable<T>(Action<T> onReceive) where T : class, new()
        // {
        //     _packetsListener.SubscribeReusable(onReceive);
        // }
        
        // public void SubscribeNetSerializable<T, TUserData>(
        //     Action<T, TUserData> onReceive) where T : INetSerializable, new()
        // {
        //     _packetsListener.SubscribeNetSerializable(onReceive);
        // }
        
        // public void SubscribeReusable<T, TUserData>(Action<T, TUserData> onReceive) where T : class, new()
        // {
        //     _packetProcessor.SubscribeReusable(onReceive);
        // }
        //
        // public void SendPacket<T>(T packet, DeliveryMethod deliveryMethod) where T : class, new()
        // {
        //     _packetsSender.SendPacket(packet, deliveryMethod);
        // }
        
        public void SendPacketSerialized<T>(PacketTypeC2S type, T packet, DeliveryMethod deliveryMethod) where T : INetSerializable
        {
            if (PlaybackSettings.IsPlaybackEnabled) return;
            _packetsSender.SendPacketSerialized(type, packet, deliveryMethod);
        }

        public void PollEvents()
        {
            if (PlaybackSettings.IsPlaybackEnabled)
            {
                // Fetch packets from Local Bridge
                while (LocalPacketBridge.TryGetNextPacket(out var packet))
                {
                    // Create Reader for FullTickPacket
                    // Need to serialize it to bytes first to simulate receipt?
                    // Or modify OnPacketReceived to take object?
                    // LiteNetLib Reader wraps bytes.

                    // We need to trigger `_packetsListener.OnNetworkReceive`.
                    // But that expects `NetPacketReader`.
                    // So we must serialize `packet` to a buffer.

                    // FullTickPacket implements INetSerializable.
                    NetDataWriter writer = new NetDataWriter();
                    // First byte is PacketType (from S2C perspective)
                    // Wait, NetworkS2CPacketsListener expects PacketTypeS2C.
                    // PacketTypeS2C.FullTick
                    writer.Put((byte)Core.Game.Domains.GamePlay.Shared.C2SModels.PacketTypeS2C.FullTick);
                    packet.Serialize(writer);

                    NetPacketReader reader = new NetPacketReader(writer.Data);
                    // Fake Peer?
                    // NetworkS2CPacketsListener doesn't really use the peer for much except maybe logging or state.
                    // Pass null or dummy?
                    // OnNetworkReceive uses peer?
                    // NetworkS2CPacketsListener.OnNetworkReceive:
                    // var packetType = (PacketTypeS2C)reader.GetByte();
                    // _packetsObservers[packetType].OnPacketReceived(reader, peer);

                    // So we can pass null if observers handle it.
                    // FullTickPacketsHandler:
                    // OnPacketReceived(NetPacketReader reader, NetPeer peer)
                    // -> _fullTickPacket.Deserialize(reader);
                    // -> ProcessStateLatestTick...
                    // It doesn't use peer.

                    _packetsListener.OnNetworkReceive(null, reader, 0, DeliveryMethod.ReliableOrdered);
                }
                return;
            }
            _netManager.PollEvents();
        }

        public void RegisterPacketsObserver(IPacketsObserver packetsObserver)
        {
            _packetsListener.RegisterObserver(packetsObserver);
        }

        public void UnregisterPacketsObserver(IPacketsObserver packetsObserver)
        {
            _packetsListener.UnregisterObserver(packetsObserver);
        }

        public void InitExitPoint()
        {
            _netManager.Stop();
            _packetsListener.OnPeerConnected -= OnServerPeerReceived;
            _updateSubscriptionService.UnregisterGuiUpdatable(this);
        }

        public void ManagedOnGUI()
        {
//            GUI.Label(new Rect(10, 10, 400, 30), "Local Host Ping: "+_packetsListener.PingToLocalHost, _guiStyle);
        }

        public void ManagedOnDrawGizmos()
        {
            
        }
    }
}