using System;
using Core.Game.Domains.GamePlay.Shared.C2SModels;
using Core.Game.Domains.GamePlay.Shared.ServerToClientModels;
using Core.Game.Domains.GamePlay.Simulation.NetworkManager.PacketsHandlers;
using Core.Scripts.Network;
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
        private readonly ClientNetworkTickProcessor _clientNetworkTickProcessor;
        private readonly NetworkC2SPacketsSender _packetsSender;
        private readonly ClientSimulationStateHandler _simulationStateHandler;
        public event Action OnClientStarted;
        public ClientNetworkManager(NetworkConfig networkConfig, IStateMachineService stateMachineService, IUpdateSubscriptionService updateSubscriptionService)
        {
            _networkConfig = networkConfig;
            _stateMachineService = stateMachineService;
            var packetProcessor = new NetPacketProcessor();
            _packetsListener = new NetworkS2CPacketsListener(packetProcessor);
            _simulationStateHandler = new ClientSimulationStateHandler(_packetsListener);
            _packetsSender = new NetworkC2SPacketsSender(packetProcessor);
            _netManager = new NetManager(_packetsListener)
            {
                AutoRecycle = true,
                IPv6Enabled = IPv6Mode.Disabled
            };
            _clientNetworkTickProcessor = new ClientNetworkTickProcessor(_netManager, _simulationStateHandler, updateSubscriptionService); 
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
            _clientNetworkTickProcessor.StartTick(_networkConfig.TicksPerSeconds, _stateMachineService.CurrentState().CancellationTokenSource);
            _netManager.Connect(_networkConfig.IpAddress, _networkConfig.Port, _networkConfig.ConntectionKey);
            OnClientStarted?.Invoke();
        }

        private void OnServerPeerReceived(NetPeer peerToServer)
        {
            _packetsSender.SetPeer(peerToServer);
            _packetsSender.SendPacketSerialized(PacketTypeC2S.JoinRequest, new JoinRequestPacketC2S { UserName = "RazPlayer" }, DeliveryMethod.ReliableOrdered);
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
        
        public void InitExitPoint()
        {
            _netManager.Stop();
            _packetsListener.OnPeerConnected -= OnServerPeerReceived;
            _clientNetworkTickProcessor.StopTick();
        }
    }
}