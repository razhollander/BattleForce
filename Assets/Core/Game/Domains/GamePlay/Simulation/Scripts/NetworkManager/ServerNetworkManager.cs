using Core.Game.Domains.GamePlay.Shared.C2SModels;
using Core.Game.Domains.GamePlay.Shared.Extensions;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager.TickHandlers.PacketsObservers;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Playback;
using Core.Scripts.Network;
using CoreDomain.Scripts.Services.Logger.Base;
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
        private readonly IPlaybackRecorderService _playbackRecorderService;
        private readonly NetworkS2CPacketsSender _packetsSender;
        private bool _didSwitchToPlayback;

        public ServerNetworkManager(NetworkConfig networkConfig, IPlaybackRecorderService playbackRecorderService)
        {
            _networkConfig = networkConfig;
            _playbackRecorderService = playbackRecorderService;
            _packetProcessor = new NetPacketProcessor();
            _packetsListener = new NetworkC2SPacketsListener(_networkConfig, _playbackRecorderService);
            _netManager = new NetManagerWrapper(_packetsListener);
            _packetsSender = new NetworkS2CPacketsSender(_packetProcessor);
        }

        public void InitEntryPoint()
        {
            RegisterAutoSerializedTypes();
            StartServer();
        }
        
        private void RegisterAutoSerializedTypes()
        {
            _packetProcessor.RegisterNestedType((w, v) => w.Put(v), r => r.GetVector2());
        }
        
        private void StartServer()
        {
            if (_netManager.IsRunning)
            {
                LogService.LogError("Server already running!");
                return;
            }
            
            _netManager.Start(_networkConfig.HostPort);
        }

        public void InitExitPoint()
        {
            _netManager.Stop();
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

        public void AddPlayerPeer(ushort playerId, NetPeer peer)
        {
            _packetsSender.AddPlayerPeer(playerId, peer);
        }

        public void PollEvents()
        {
            _netManager.PollEvents();
        }

        public int GetPlayerPeerId(ushort playerId)
        {
            return _packetsSender.GetPlayerPeerId(playerId);
        }

        public void RegisterPacketsObserver(IPacketsObserver packetsObserver)
        {
            _packetsListener.RegisterObserver(packetsObserver);
        }
        
        public void UnregisterPacketsObserver(IPacketsObserver packetsObserver)
        {
            _packetsListener.UnregisterObserver(packetsObserver);
        }

        public void SwitchToPlayback()
        {
            if (!_didSwitchToPlayback)
            {
                _netManager = new NetManagerPlayback(_packetsListener, _playbackRecorderService);
            }
            _didSwitchToPlayback = true;
        }
    }
}