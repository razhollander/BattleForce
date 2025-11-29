using System;
using Core.Game.Domains.GamePlay.Shared.C2SModels;
using Core.Game.Domains.GamePlay.Shared.C2SModels.Packets;
using Core.Scripts.Network;
using CoreDomain.Scripts.Services.CommandFactory;
using CoreDomain.Scripts.Services.Logger.Base;
using CoreDomain.Scripts.Services.StateMachineService;
using CoreDomain.Scripts.Services.UpdateService;
using LiteNetLib;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Presentation.Scripts.Network
{
    public class ClientNetworkManager : IClientNetworkManager
    {
        private readonly NetworkS2CPacketsListener _packetsListener;
        private readonly NetManager _netManager;
        private readonly NetworkConfig _networkConfig;
        private readonly IStateMachineService _stateMachineService;
        private readonly ICommandFactory _commandFactory;
        private readonly NetworkC2SPacketsSender _packetsSender;
        public bool IsPeerConnected { get; private set; }

        public ClientNetworkManager(NetworkConfig networkConfig, IStateMachineService stateMachineService, ICommandFactory commandFactory)
        {
            _networkConfig = networkConfig;
            _stateMachineService = stateMachineService;
            _commandFactory = commandFactory;
            var packetProcessor = new NetPacketProcessor();
            _packetsListener = new NetworkS2CPacketsListener(packetProcessor);
            _packetsSender = new NetworkC2SPacketsSender(packetProcessor);
            _netManager = new NetManager(_packetsListener)
            {
                AutoRecycle = true,
                IPv6Enabled = IPv6Mode.Disabled
            };
        }

        public void StartClient()
        {
            if (_netManager.IsRunning)
            {
                LogService.LogError("Client already running!");
                return;
            }
            
            _netManager.Start();
            //_packetsListener.RegisterListeners();
            _packetsListener.OnPeerConnected += OnServerPeerReceived;
            _netManager.Connect(_networkConfig.IpAddress, _networkConfig.Port, _networkConfig.ConntectionKey);
        }

        private void OnServerPeerReceived(NetPeer peerToServer)
        {
            _packetsSender.SetPeer(peerToServer);
            _commandFactory.CreateCommandVoid<HandleClientConnectedToPeerCommand>();
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
        }
    }
}