using System;
using System.Collections.Generic;
using System.IO;
using Core.Game.Domains.GamePlay.Shared.C2SModels;
using Core.Game.Domains.GamePlay.Shared.C2SModels.Packets;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager.TickHandlers.PacketsObservers;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Services.SimulationPersistentData;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Services.TickService;
using Core.Scripts.Extensions;
using Core.Scripts.Utils;
using CoreDomain.Scripts.Services.DataPersistence;
using CoreDomain.Scripts.Services.Logger.Base;
using LiteNetLib;
using LiteNetLib.Utils;
using Newtonsoft.Json;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Playback
{
    public class PlaybackRecorderService : IPlaybackRecorderService, IRawPacketsObserver
    {
        private readonly ITickService _tickService;
        private readonly IServerNetworkManager _networkManager;
        private readonly ISimulationPersistentData _simulationPersistentData;
        private Dictionary<int, PlaybackTickData> _ticks = new Dictionary<int, PlaybackTickData>();
        private int _seed;
        private readonly string _jsonFilePath;
        private readonly string _debugFilePath;
        private int _initialTick;

        public int Seed => _seed;
        public int InitialTick => _initialTick;
        public bool IsPlaybackEnabled { get; private set; }
        public bool IsRecordingEnabled { get; set; }

        public PlaybackRecorderService(ITickService tickService, IServerNetworkManager networkManager, ISimulationPersistentData simulationPersistentData)
        {
            _tickService = tickService;
            _networkManager = networkManager;
            _simulationPersistentData = simulationPersistentData;
            var directory = Directory.GetCurrentDirectory() + "/Records";
            _jsonFilePath = Path.Combine(directory, "playback.json");
            _debugFilePath = Path.Combine(directory, "playback_debug.json");
            Directory.CreateDirectory(directory);
        }

        public void InitEntryPoint()
        {
            IsPlaybackEnabled = _simulationPersistentData.IsPlaybackEnabled;

            if (!IsPlaybackEnabled)
            {
                _networkManager.RegisterPacketsObserver(this);
            }
        }

        public void InitExitPoint()
        {
            if (!IsPlaybackEnabled)
            {
                _networkManager.UnregisterPacketsObserver(this);
                SaveRecording();
            }
        }

        public void StartRecording(int seed)
        {
            _seed = seed;
            _ticks.Clear();
            _initialTick = _tickService.CurrentTick;
            LogService.LogTopic($"Started Recording Playback. Seed: {seed}", LogTopicType.ServerNetwork);
        }

        public void SaveRecording()
        {
            SaveJson();
            SaveDebugJson();
            LogService.LogError("Saved Records!");
        }

        private void SaveJson()
        {
            try
            {
                var fileData = new PlaybackFile
                {
                    InitialTick = _initialTick,
                    Seed = _seed,
                    Ticks = _ticks
                };

                string json = JsonConvert.SerializeObject(fileData);
                File.WriteAllText(_jsonFilePath, json);
                LogService.LogTopic($"Saved Playback to {_jsonFilePath}", LogTopicType.ServerNetwork);
            }
            catch (Exception e)
            {
                LogService.LogError($"Failed to save playback: {e}");
            }
        }

        private void SaveDebugJson()
        {
            try
            {
                var debugData = new PlaybackDebugData { Seed = _seed , InitialTick = _initialTick};
                var sortedKeys = new List<int>(_ticks.Keys);

                foreach (var tick in sortedKeys)
                {
                    var tickData = _ticks[tick];
                    var debugTick = new DebugTickData();
                    foreach (var packet in tickData.Packets)
                    {
                        var debugPacket = new DebugRecordedPacket { PlayerId = packet.PlayerId };

                        // Attempt to deserialize as PlayerInputPacketC2S
                        // We need to skip the first byte (PacketType)
                        if (packet.Data.Length > 1)
                        {
                            // 0 = PacketType
                            // Rest is data
                            // Assuming PacketTypeC2S.PlayerInput (check byte if needed)
                             // PacketTypeC2S.PlayerInput is likely not 0, need to check enum.
                             // But regardless, the requirement is to save human readable JSON.

                             NetDataReader reader = new NetDataReader(packet.Data);
                             var b = reader.GetByte();
                            
                             PacketTypeC2S packetType = (PacketTypeC2S) (int) b;
                            
                             switch (packetType)
                             {
                                 case PacketTypeC2S.MatchPlayerInput: 
                                     var matchInputPacket = new MatchPlayerInputPacketC2S();
                                     matchInputPacket.Deserialize(reader);
                                     debugPacket.PacketData = JsonConvert.SerializeObject(matchInputPacket);
                                     break;
                                 case PacketTypeC2S.MatchMakingPlayerInput: 
                                     var matchMakingInputPacket = new MatchMakingPlayerInputPacketC2S();
                                     matchMakingInputPacket.Deserialize(reader);
                                     debugPacket.PacketData = JsonConvert.SerializeObject(matchMakingInputPacket);
                                     break;
                                 default: LogService.LogError("packet not recorded of type: " + packetType + ""); break;
                             }
                        }

                        debugTick.Packets.Add(debugPacket);
                    }
                    debugData.Ticks.Add(debugTick);
                }

                string json = JsonConvert.SerializeObject(debugData, Formatting.Indented);
                File.WriteAllText(_debugFilePath, json);
                LogService.LogTopic($"Saved Debug Playback to {_debugFilePath}", LogTopicType.ServerNetwork);
            }
            catch (Exception e)
            {
                LogService.LogError($"Failed to save debug playback: {e}");
            }
        }

        public void LoadRecording()
        {
            if (!File.Exists(_jsonFilePath))
            {
                LogService.LogError($"Playback file not found at {_jsonFilePath}");
                return;
            }

            try
            {
                _ticks.Clear();
                string json = File.ReadAllText(_jsonFilePath);
                var fileData = JsonConvert.DeserializeObject<PlaybackFile>(json);
                _seed = fileData.Seed;
                _ticks = fileData.Ticks;
                _initialTick = fileData.InitialTick;
                LogService.LogTopic($"Loaded Playback. Seed: {_seed}, Ticks: {_ticks.Count}", LogTopicType.ServerNetwork);
            }
            catch (Exception e)
            {
                LogService.LogError($"Failed to load playback: {e}");
            }
        }

        public List<RecordedPacket> GetPacketsForTick(int tick)
        {
            if (_ticks.TryGetValue(tick, out var data))
            {
                return data.Packets;
            }
            return null;
        }

        public void OnPacketReceived(byte[] packetBytes, NetPeer peer)
        {
            if (peer.Tag == null)
            {
                return;
            }
            
            var playerId = (ushort)peer.Tag;
            RecordPacket(playerId, packetBytes);
        }
        
        public void RecordPacket(ushort playerId, byte[] data)
        {
            var serverTick = _tickService.CurrentTick;
            if (!_ticks.TryGetValue(serverTick, out var tickData))
            {
                tickData = new PlaybackTickData { Tick = serverTick };
                _ticks[serverTick] = tickData;
            }

            tickData.Packets.Add(new RecordedPacket
            {
                PlayerId = playerId,
                Data = data
            });
        }
    }
}
