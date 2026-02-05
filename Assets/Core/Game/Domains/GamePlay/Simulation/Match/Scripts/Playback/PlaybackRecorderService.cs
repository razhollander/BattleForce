using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Core.Game.Domains.GamePlay.Shared.C2SModels;
using Core.Game.Domains.GamePlay.Shared.C2SModels.Packets;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Playback;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager.TickHandlers.PacketsObservers;
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
        private readonly SimulationGamePlayConfig _gamePlayConfig;
        private Dictionary<int, PlaybackTickData> _ticks = new Dictionary<int, PlaybackTickData>();
        private int _seed;
        private int _initialTick;
        private SimulationMatchEnterData.PlayerData[] _players;
        private string _playbackFileName;

        public int Seed => _seed;
        public int InitialTick => _initialTick;
        public bool IsPlaybackEnabled { get; private set; }
        public SimulationMatchEnterData.PlayerData[] LoadedPlayers => _players;

        public PlaybackRecorderService(ITickService tickService, IServerNetworkManager networkManager, SimulationGamePlayConfig gamePlayConfig)
        {
            _tickService = tickService;
            _networkManager = networkManager;
            _gamePlayConfig = gamePlayConfig;
        }

        public void SetPlaybackInfo(bool isEnabled, string playbackFileName)
        {
            IsPlaybackEnabled = isEnabled;
            _playbackFileName = playbackFileName;
        }

        public void InitEntryPoint()
        {
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

        public void StartRecording(int seed, SimulationMatchEnterData.PlayerData[] players)
        {
            _seed = seed;
            _players = players;
            _ticks.Clear();
            _initialTick = _tickService.CurrentTick;
            LogService.LogTopic($"Started Recording Playback. Seed: {seed}", LogTopicType.ServerNetwork);
        }

        public void SaveRecording()
        {
            SaveJson();
            LogService.LogError("Saved Records!");
        }

        private void SaveJson()
        {
            try
            {
                var directory = Path.Combine(Directory.GetCurrentDirectory(), "Records");
                Directory.CreateDirectory(directory);

                var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
                var fileName = $"playback_{timestamp}.json";
                var filePath = Path.Combine(directory, fileName);

                var fileData = new PlaybackFile
                {
                    InitialTick = _initialTick,
                    Seed = _seed,
                    Ticks = _ticks,
                    Players = _players
                };

                string json = JsonConvert.SerializeObject(fileData);
                File.WriteAllText(filePath, json);
                LogService.LogTopic($"Saved Playback to {filePath}", LogTopicType.ServerNetwork);

                ManageSavedPlaybacks(directory);
            }
            catch (Exception e)
            {
                LogService.LogError($"Failed to save playback: {e}");
            }
        }

        private void ManageSavedPlaybacks(string directory)
        {
            try
            {
                var files = Directory.GetFiles(directory, "playback_*.json")
                    .OrderBy(f => File.GetCreationTime(f))
                    .ToList();

                while (files.Count > _gamePlayConfig.MaxSavedPlaybacks)
                {
                    File.Delete(files[0]);
                    files.RemoveAt(0);
                }
            }
            catch (Exception e)
            {
                LogService.LogError($"Failed to manage saved playbacks: {e}");
            }
        }

        public void LoadRecording()
        {
            var directory = Path.Combine(Directory.GetCurrentDirectory(), "Records");
            var filePath = Path.Combine(directory, _playbackFileName);

            if (!File.Exists(filePath))
            {
                LogService.LogError($"Playback file not found at {filePath}");
                return;
            }

            try
            {
                _ticks.Clear();
                string json = File.ReadAllText(filePath);
                var fileData = JsonConvert.DeserializeObject<PlaybackFile>(json);
                _seed = fileData.Seed;
                _ticks = fileData.Ticks;
                _initialTick = fileData.InitialTick;
                _players = fileData.Players;
                LogService.LogTopic($"Loaded Playback. Seed: {_seed}, Ticks: {_ticks.Count}, Players: {_players?.Length}", LogTopicType.ServerNetwork);
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
