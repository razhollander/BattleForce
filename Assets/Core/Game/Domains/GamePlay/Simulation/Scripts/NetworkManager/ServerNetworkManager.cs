using System;
using Core.Game.Domains.GamePlay.Shared.C2SModels;
using Core.Game.Domains.GamePlay.Shared.Extensions;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager.TickHandlers.PacketsObservers;
using Core.Scripts.Network;
using LiteNetLib;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager
{
    public class ServerNetworkManager : IServerNetworkManager
    {
        private readonly NetworkC2SPacketsListener _packetsListener;
        private INetManagerWrapper _netManager;
        private readonly NetPacketProcessor _packetProcessor;
        private readonly NetworkConfig _networkConfig;
        private readonly NetworkS2CPacketsSender _packetsSender;

        public int ConnectedPeersCount => _netManager.ConnectedPeersCount;
        public event Action OnPacketReceivedEvent;
        public event Action<ushort> OnPeerDisconnectedEvent;

        public ServerNetworkManager(NetworkConfig networkConfig)
        {
            _networkConfig = networkConfig;
            _packetProcessor = new NetPacketProcessor();
            _packetsListener = new NetworkC2SPacketsListener(_networkConfig);
            _netManager = new NetManagerWrapper();
            _netManager.SetPacketsListener(_packetsListener);
            _packetsSender = new NetworkS2CPacketsSender(_packetProcessor);
        }
        
        public void InitEntryPoint(int port)
        {
            AddListeners();
            RegisterAutoSerializedTypes();
            StartServer(port);
        }

        private void AddListeners()
        {
            _packetsListener.OnPacketReceivedEvent += OnPacketReceived;
            _packetsListener.OnPeerDisconnectedEvent += OnPeerDisconnected;
        }
        
        private void OnPacketReceived()
        {
            OnPacketReceivedEvent?.Invoke();
        }

        private void OnPeerDisconnected(ushort playerId)
        {
            OnPeerDisconnectedEvent?.Invoke(playerId);
        }

        
        private void RegisterAutoSerializedTypes()
        {
            _packetProcessor.RegisterNestedType((w, v) => w.Put(v), r => r.GetVector2());
        }
        
        private void StartServer(int port)
        {
            // if (_netManager.IsRunning)
            // {
            //     LogService.LogError("Server already running!");
            //     return;
            // }
            
            _netManager.Start(port);
        }

        public void InitExitPoint()
        {
            RemoveListeners();
            _netManager.Stop();
        }

        private void RemoveListeners()
        {
            _packetsListener.OnPacketReceivedEvent -= OnPacketReceived;
            _packetsListener.OnPeerDisconnectedEvent -= OnPeerDisconnected;
        }

        // public void SendToAllPlayersPacketSerialized<T>(PacketTypeS2C type, T packet, DeliveryMethod deliveryMethod) where T : INetSerializable
        // {
        //     _packetsSender.SendPacketToAllPlayersSerialized(type, packet, deliveryMethod);
        // }

        public void SendPacketToPlayerSerialized<T>(ushort playerId, PacketTypeS2C type, T packet,
            DeliveryMethod deliveryMethod) where T : INetSerializable
        {
            _packetsSender.SendPacketToPlayerSerialized(playerId, type, packet, deliveryMethod);
        }

        public void SendPacketToPeerSerialized<T>(NetPeer peer, PacketTypeS2C type, T packet, DeliveryMethod deliveryMethod) where T : INetSerializable
        {
            _packetsSender.SendPacketToPeerSerialized(peer, type, packet, deliveryMethod);
        }

        public void AddPlayerPeer(ushort playerId, NetPeer peer)
        {
            _packetsSender.AddPlayerPeer(playerId, peer);
        }

        public void RemovePlayerPeer(ushort playerId)
        {
            _packetsSender.RemovePlayerPeer(playerId);
        }

        public void PollEvents()
        {
            _netManager.PollEvents();
        }

        public int GetPlayerPeerId(ushort playerId)
        {
            return _packetsSender.GetPlayerPeerId(playerId);
        }  
        
        public bool IsPlayerPeerConencted(ushort playerId)
        {
            return _packetsSender.IsPlayerConnected(playerId);
        }

        public void RegisterPacketsObserver(IPacketsObserver packetsObserver)
        {
            _packetsListener.RegisterObserver(packetsObserver);
        }
        
        public void UnregisterPacketsObserver(IPacketsObserver packetsObserver)
        {
            _packetsListener.UnregisterObserver(packetsObserver);
        }
        
        public void RegisterPacketsObserver(IRawPacketsObserver packetsObserver)
        {
            _packetsListener.RegisterObserver(packetsObserver);
        }
        
        public void UnregisterPacketsObserver(IRawPacketsObserver packetsObserver)
        {
            _packetsListener.UnregisterObserver(packetsObserver);
        }

        public void SwitchToNetManager(INetManagerWrapper netManagerWrapper)
        {
            netManagerWrapper.SetPacketsListener(_packetsListener);
            _netManager = netManagerWrapper;
        }
    }
}