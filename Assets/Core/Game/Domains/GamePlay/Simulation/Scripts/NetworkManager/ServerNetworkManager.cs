using System;
using Core.Game.Domains.GamePlay.Presentation.Scripts.Network;
using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Shared.C2SModels;
using Core.Game.Domains.GamePlay.Shared.Extensions;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Playback;
using Core.Scripts.Network;
using Core.Scripts.Utils;
using CoreDomain.Scripts.Services.Logger.Base;
using LiteNetLib;
using LiteNetLib.Utils;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Shared.NetworkManager
{
    public class ServerNetworkManager : IServerNetworkManager
    {
        private readonly NetworkC2SPacketsListener _packetsListener;
        private readonly NetManager _netManager;
        private readonly NetPacketProcessor _packetProcessor;
        private readonly NetworkConfig _networkConfig;
        private readonly NetworkS2CPacketsSender _packetsSender;
        private PlaybackService _playbackService;

        public int ServerTick { get; private set; }

        public ServerNetworkManager(NetworkConfig networkConfig)
        {
            _networkConfig = networkConfig;
            _packetProcessor = new NetPacketProcessor();
            _packetsListener = new NetworkC2SPacketsListener(_networkConfig);
            _netManager = new NetManager(_packetsListener) { AutoRecycle = true, BroadcastReceiveEnabled = true, IPv6Enabled = false};
            _packetsSender = new NetworkS2CPacketsSender(_packetProcessor);
        }

        public void SetPlaybackService(PlaybackService playbackService)
        {
            _playbackService = playbackService;
            _packetsListener.SetPlaybackService(playbackService);
        }

        public void SetServerTick(int tick)
        {
            ServerTick = tick;
            _packetsListener.SetCurrentTick(tick);
        }

        public void InitEntryPoint()
        {
            RegisterAutoSerializedTypes();
            if (!PlaybackSettings.IsPlaybackEnabled)
            {
                StartServer();
            }
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
            if (PlaybackSettings.IsPlaybackEnabled)
            {
                // Bridge to Local Client if possible
                if (packet is Core.Game.Domains.GamePlay.Shared.S2CModels.FullTickPacket fullTickPacket)
                {
                    LocalPacketBridge.SendToClient(fullTickPacket);
                }
                return;
            }
            _packetsSender.SendPacketToPlayerSerialized(playerId, type, packet, deliveryMethod);
        }

        public void AddPlayerPeer(ushort playerId, NetPeer peer)
        {
            _packetsSender.AddPlayerPeer(playerId, peer);
        }

        public void PollEvents()
        {
            if (PlaybackSettings.IsPlaybackEnabled)
            {
                // Playback Logic
                var packets = _playbackService.GetPacketsForTick(ServerTick);
                if (packets != null)
                {
                    foreach (var p in packets)
                    {
                        // Create Dummy Peer
                        // We need a way to mock NetPeer.
                        // NetPeer constructor is internal?
                        // We can't easily mock NetPeer.
                        // But _packetsListener.OnNetworkReceive uses peer.Tag.
                        // We can subclass NetPeer? No, it's sealed or internal ctor.

                        // Reflection to create NetPeer?
                        // Or wrapper.

                        // Wait, LiteNetLib NetPeer is hard to mock.
                        // However, `OnNetworkReceive` takes `NetPeer`.
                        // If I can't instantiate it, I'm stuck.

                        // Hack: `NetManager` keeps a list of peers.
                        // If I didn't start the server, I have no peers.

                        // Maybe I can modify `IPacketsObserver.OnPacketReceived` to take `ushort playerId` instead of `NetPeer`?
                        // But `NetPeer` is used elsewhere.

                        // Let's check usages of `peer` in `PlayerInputsPacketsHandler`.
                        // `var playerId = (ushort)peer.Tag;`
                        // That's it.

                        // So I can create a fake class that acts like NetPeer? No, `NetPeer` is a class.
                        // I can use `FormatterServices.GetUninitializedObject(typeof(NetPeer))`?
                        // And set the Tag field via reflection.

                        var dummyPeer = (NetPeer)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(NetPeer));
                        // Set Tag
                        if (p.PlayerId != ushort.MaxValue)
                        {
                            dummyPeer.Tag = p.PlayerId;
                        }

                        NetPacketReader reader = new NetPacketReader(p.Data);
                        _packetsListener.OnNetworkReceive(dummyPeer, reader, 0, DeliveryMethod.ReliableOrdered);
                    }
                }
            }
            else
            {
                _netManager.PollEvents();
            }
        }

        public int GetPlayerPeerId(ushort playerId)
        {
            if (PlaybackSettings.IsPlaybackEnabled) return 0;
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
    }
}