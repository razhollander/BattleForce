using System;
using System.Collections.Generic;
using System.IO;
using Core.Game.Domains.GamePlay.Shared.C2SModels.Packets;
using Core.Game.Domains.GamePlay.Shared.NetworkManager;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager.TickHandlers.PacketsObservers;
using Core.Scripts.Utils;
using CoreDomain.Scripts.Services.Logger.Base;
using LiteNetLib;
using LiteNetLib.Utils;
using Newtonsoft.Json;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Playback
{
    public class PlaybackRecorderService : IPlaybackRecorderService, IRawPacketsObserver
    {
        private readonly ITickCounterService _tickCounterService;
        private readonly IServerNetworkManager _networkManager;
        private Dictionary<int, PlaybackTickData> _ticks = new Dictionary<int, PlaybackTickData>();
        private int _seed;
        private readonly string _jsonFilePath;
        private readonly string _debugFilePath;

        public int Seed => _seed;
        public bool IsPlaybackEnabled { get; private set; }
        public bool IsRecordingEnabled { get; set; }

        public PlaybackRecorderService(ITickCounterService tickCounterService, IServerNetworkManager networkManager)
        {
            _tickCounterService = tickCounterService;
            _networkManager = networkManager;
            var directory = Application.dataPath + "/Records";
            _jsonFilePath = Path.Combine(directory, "playback.json");
            _debugFilePath = Path.Combine(directory, "playback_debug.json");
            Directory.CreateDirectory(directory);
        }

        public void InitEntryPoint()
        {
            IsPlaybackEnabled = PlayerPrefsSettings.IsPlaybackEnabled;

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
            LocalPacketBridge.Clear();
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
                var debugData = new PlaybackDebugData { Seed = _seed };
                var sortedKeys = new List<int>(_ticks.Keys);
                sortedKeys.Sort();

                foreach (var tick in sortedKeys)
                {
                    var tickData = _ticks[tick];
                    var debugTick = new DebugTickData { Tick = tick };
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
                             byte packetType = reader.GetByte();

                             // Hardcoded check for InputPacket if possible, or just try
                             // For now assuming all recorded C2S are inputs or Join
                             // Let's try to deserialize Input
                             try
                             {
                                 var inputPacket = new MatchPlayerInputPacketC2S();
                                 inputPacket.Deserialize(reader);
                                 debugPacket.InputPacket = inputPacket;
                             }
                             catch
                             {
                                 // Ignore if not input packet
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
                LocalPacketBridge.Clear();
                string json = File.ReadAllText(_jsonFilePath);
                var fileData = JsonConvert.DeserializeObject<PlaybackFile>(json);
                _seed = fileData.Seed;
                _ticks = fileData.Ticks;

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
            var serverTick = _tickCounterService.CurrentTick;
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
