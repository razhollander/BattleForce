using System;
using Core.Game.Domains.GamePlay.Presentation.Scripts.Network;
using Core.Game.Domains.GamePlay.Shared.C2SModels;
using Core.Game.Domains.GamePlay.Shared.ServerToClientModels;
using Core.Game.Domains.GamePlay.Simulation;
using Core.Game.Domains.GamePlay.Simulation.NetworkManager;
using Core.Scripts.Network;
using CoreDomain.Scripts.Services.Logger.Base;
using CoreDomain.Scripts.Services.StateMachineService;
using LiteNetLib;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.NetworkManager
{
    public class ServerNetworkManager : IServerNetworkManager
    {
        private NetworkC2SPacketsListener _networkC2SPacketsListener;
        private NetManager _netManager;
        private NetPacketProcessor _packetProcessor;
        private readonly NetworkConfig _networkConfig;
        private readonly IStateMachineService _stateMachineService;
        private readonly ITickProcessor _serverNetworkTickProcessor;
        private readonly ServerPlayersInputListener _serverPlayersInputListener;
        private readonly NetworkS2CPacketsSender _packetsSender;

        public ServerNetworkManager(NetworkConfig networkConfig, IStateMachineService stateMachineService)
        {
            _networkConfig = networkConfig;
            _stateMachineService = stateMachineService;
            _packetProcessor = new NetPacketProcessor();
            _networkC2SPacketsListener = new NetworkC2SPacketsListener(_packetProcessor, _networkConfig);
            _serverPlayersInputListener = new ServerPlayersInputListener(_networkC2SPacketsListener);
            _netManager = new NetManager(_networkC2SPacketsListener) { AutoRecycle = true };
            _serverNetworkTickProcessor = new ServerNetworkTickProcessor(_netManager, _serverPlayersInputListener);
            _packetsSender = new NetworkS2CPacketsSender(_packetProcessor);
        }

        public void InitEntryPoint()
        {
            StartServer();
        }
        
        private void StartServer()
        {
            if (_netManager.IsRunning)
            {
                LogService.LogError("Server already running!");
                return;
            }
            
            //_networkC2SPacketsListener.RegisterListeners();
            _netManager.Start(_networkConfig.Port);
            _serverNetworkTickProcessor.StartTick(_networkConfig.TicksPerSeconds, _stateMachineService.CurrentState().CancellationTokenSource);
        }

        public void InitExitPoint()
        {
            _netManager.Stop();
            _serverNetworkTickProcessor.StopTick();
        }

        // public void SubscribeReusable<T>(Action<T> onReceive) where T : class, new()
        // {
        //     _packetProcessor.SubscribeReusable(onReceive);
        // }

        // public void SubscribeReusable<T, TUserData>(Action<T, TUserData> onReceive) where T : class, new()
        // {
        //     _packetProcessor.SubscribeReusable(onReceive);
        // }
        
        public void SubscribeNetSerializable<T, TUserData>(
            Action<T, TUserData> onReceive) where T : INetSerializable, new()
        {
            _packetProcessor.SubscribeNetSerializable(onReceive);
        }

        // public void SendPacket<T>(T packet, DeliveryMethod deliveryMethod) where T : class, new()
        // {
        //     _packetsSender.SendPacket(packet, deliveryMethod);
        // }
        
        public void SendPacketSerialized<T>(PacketTypeS2C type, T packet, DeliveryMethod deliveryMethod) where T : INetSerializable
        {
            _packetsSender.SendPacketSerialized(type, packet, deliveryMethod);
        }

        // public void SendPacketOnlyToPlayer<T>(T packet, DeliveryMethod deliveryMethod, int playerId)
        //     where T : class, new()
        // {
        //     _packetsSender.SendPacketOnlyToPlayer(packet, deliveryMethod, playerId);
        // }

        public void SendPacketSerializedOnlyToPlayer<T>(PacketTypeS2C type, T packet, int playerId,
            DeliveryMethod deliveryMethod) where T : INetSerializable
        {
            _packetsSender.SendPacketSerializedOnlyToPlayer(type, packet, playerId, deliveryMethod);
        }

        public void RemoveSubscription<T>()
        {
            _packetProcessor.RemoveSubscription<T>();
        }

        public void AddPlayerPeer(int playerId, NetPeer peer)
        {
            _packetsSender.AddPlayerPeer(playerId, peer);
        }
    }
}