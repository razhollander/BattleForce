using System;
using Core.Game.Domains.GamePlay.Shared.C2SModels;
using Core.Scripts.Network;
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
        public bool IsPeerConnected { get; private set; }
        public int Ping => _packetsSender.Peer.Ping;
        public int LocalPeerId => _packetsSender.Peer.Id;

        public ClientNetworkManager(NetworkConfig networkConfig, ICommandFactory commandFactory, IUpdateSubscriptionService updateSubscriptionService)
        {
            _networkConfig = networkConfig;
            _commandFactory = commandFactory;
            _updateSubscriptionService = updateSubscriptionService;
            var packetProcessor = new NetPacketProcessor();
            _packetsListener = new NetworkS2CPacketsListener(packetProcessor);
            _packetsSender = new NetworkC2SPacketsSender(packetProcessor);
            _netManager = new NetManager(_packetsListener)
            {
                AutoRecycle = true,
                IPv6Enabled = IPv6Mode.Disabled
            };
        }

        public void StartClient(bool isHost)
        {
            if (_netManager.IsRunning)
            {
                LogService.LogError("Client already running!");
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
#if Logs
            LogService.LogTopic("Server peer received!", LogTopicType.ClientNetwork);
#endif
            _packetsSender.SetPeer(peerToServer);
            _commandFactory.CreateCommandVoid<HandleClientConnectedToPeerCommand>().Execute();
            IsPeerConnected = true;
        }

        // public void SubscribeReusable<T>(Action<T> onReceive) where T : class, new()
        // {
        //     _packetsListener.SubscribeReusable(onReceive);
        // }
        
        public void SubscribeNetSerializable<T, TUserData>(
            Action<T, TUserData> onReceive) where T : INetSerializable, new()
        {
            _packetsListener.SubscribeNetSerializable(onReceive);
        }
        
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
            _packetsSender.SendPacketSerialized(type, packet, deliveryMethod);
        }

        public void RemoveSubscription<T>()
        {
            _packetsListener.RemoveSubscription<T>();
        }

        public void PollEvents()
        {
            _netManager.PollEvents();
        }

        public void InitExitPoint()
        {
            _netManager.Stop();
            _packetsListener.OnPeerConnected -= OnServerPeerReceived;
            _updateSubscriptionService.UnregisterGuiUpdatable(this);
        }

        public void ManagedOnGUI()
        {
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.fontSize = 10;
            style.normal.textColor = Color.white;
            GUI.Label(new Rect(10, 10, 400, 30), "Local Host Ping: "+_packetsListener.PingToLocalHost, style);
        }

        public void ManagedOnDrawGizmos()
        {
            
        }
    }
}