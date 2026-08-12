using System;
using Core.Game.Domains.GamePlay.Shared.C2SModels;
using Core.Game.Domains.GamePlay.Shared.Extensions;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager.TickHandlers.PacketsObservers;
using Core.Scripts.Network;
using CoreDomain.Scripts.Services.CommandFactory;
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
        private readonly ICommandFactory _commandFactory;

        private HandleClientDisconnectedCommand _handleClientDisconnectedCommand;

        public int ConnectedPeersCount => _netManager.ConnectedPeersCount;
        public event Action OnPacketReceivedEvent;
        public event Action<long> OnClientPeerDisconnectedEvent;

        public ServerNetworkManager(NetworkConfig networkConfig, ICommandFactory commandFactory)
        {
            _networkConfig = networkConfig;
            _commandFactory = commandFactory;
            _packetProcessor = new NetPacketProcessor();
            _packetsListener = new NetworkC2SPacketsListener(_networkConfig);
            _netManager = new NetManagerWrapper();
            _netManager.SetPacketsListener(_packetsListener);
            _packetsSender = new NetworkS2CPacketsSender(_packetProcessor);
        }

        public void InitEntryPoint(int port)
        {
            _handleClientDisconnectedCommand = _commandFactory.CreateCommandVoid<HandleClientDisconnectedCommand>();
            AddListeners();
            RegisterAutoSerializedTypes();
            StartServer(port);
        }

        private void AddListeners()
        {
            _packetsListener.OnPacketReceivedEvent += OnPacketReceived;
            _packetsListener.OnClientPeerDisconnectedEvent += OnClientPeerDisconnected;
        }

        private void OnPacketReceived()
        {
            OnPacketReceivedEvent?.Invoke();
        }

        private void OnClientPeerDisconnected(long clientId)
        {
            _handleClientDisconnectedCommand.SetClientId(clientId).Execute();
            OnClientPeerDisconnectedEvent?.Invoke(clientId);
        }

        
        private void RegisterAutoSerializedTypes()
        {
            _packetProcessor.RegisterNestedType((w, v) => w.Put(v), r => r.GetVector2());
        }
        
        private void StartServer(int port)
        {
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
            _packetsListener.OnClientPeerDisconnectedEvent -= OnClientPeerDisconnected;
        }

        public void SendPacketToClientSerialized<T>(long clientId, PacketTypeS2C type, T packet,
            DeliveryMethod deliveryMethod) where T : INetSerializable
        {
            _packetsSender.SendPacketToClientSerialized(clientId, type, packet, deliveryMethod);
        }

        public void SendPacketToPeerSerialized<T>(NetPeer peer, PacketTypeS2C type, T packet, DeliveryMethod deliveryMethod) where T : INetSerializable
        {
            _packetsSender.SendPacketToPeerSerialized(peer, type, packet, deliveryMethod);
        }

        public void AddClientPeer(long clientId, NetPeer peer)
        {
            _packetsSender.AddClientPeer(clientId, peer);
        }

        public void RemoveClientPeer(long clientId)
        {
            _packetsSender.RemoveClientPeer(clientId);
        }

        public void PollEvents()
        {
            _netManager.PollEvents();
        }

        public bool TryGetClientPeerId(long clientId, out int peerId)
        {
            return _packetsSender.TryGetClientPeerId(clientId, out peerId);
        }  
        
        public bool IsClientPeerConencted(long clientId)
        {
            return _packetsSender.IsClientConnected(clientId);
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