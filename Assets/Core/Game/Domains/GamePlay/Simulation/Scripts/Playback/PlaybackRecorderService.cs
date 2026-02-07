using System;
using System.Collections.Generic;
using System.IO;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Shared.Scripts.Playback;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager.TickHandlers.PacketsObservers;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Services.TickService;
using Core.Scripts.Extensions;
using CoreDomain.Scripts.Services.Logger.Base;
using LiteNetLib;
using Newtonsoft.Json;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.Playback
{
    public class PlaybackRecorderService : IPlaybackRecorderService, IRawPacketsObserver
    {
        private readonly ITickService _tickService;
        private readonly IServerNetworkManager _networkManager;
        private readonly SimulationGamePlayConfig _gamePlayConfig;
        private readonly IPlaybackIOService _playbackIOService;
        private Dictionary<int, PlaybackTickData> _ticks = new Dictionary<int, PlaybackTickData>();
        private int _seed;
        private int _initialTick;
        
        public MatchSimulationStateS2C InitialSimulationState { get; private set; }

        public SimulationMatchEnterData.PlayerData[] LoadedPlayers
        {
            get
            {
                var playersData = new SimulationMatchEnterData.PlayerData[InitialSimulationState.Players.Count];

                for (int i = 0; i < InitialSimulationState.Players.Count; i++)
                {
                    var playerState = InitialSimulationState.Players.GetByIndex(i);
                    playersData[i].Id = playerState.Id;
                    playersData[i].TeamId = playerState.TeamId;
                    playersData[i].Name = playerState.Name;
                }

                return playersData;
            }
        }

        public int Seed => _seed;
        public int InitialTick => _initialTick;
        public bool IsPlaybackEnabled { get; private set; }

        public PlaybackRecorderService(ITickService tickService, IServerNetworkManager networkManager, SimulationGamePlayConfig gamePlayConfig, IPlaybackIOService playbackIOService)
        {
            _tickService = tickService;
            _networkManager = networkManager;
            _gamePlayConfig = gamePlayConfig;
            _playbackIOService = playbackIOService;
        }

        public void InitEntryPoint(bool isEnabled, string playbackFileName)
        {
            IsPlaybackEnabled = isEnabled;

            if (IsPlaybackEnabled && _playbackIOService.TryGetPlayback(playbackFileName, out var playbackFile))
            {
                LoadPlayback(playbackFile);
            }
        }

        public void StopRecording()
        {
            _networkManager.UnregisterPacketsObserver(this);
            SaveRecording();
        }

        public void StartRecording(int seed, MatchSimulationStateS2C initialSimulationState)
        {
            _seed = seed;
            InitialSimulationState = initialSimulationState;
            _ticks.Clear();
            _initialTick = _tickService.CurrentTick;
            _networkManager.RegisterPacketsObserver(this);
            LogService.LogTopic($"Started Recording Playback. Seed: {seed}", LogTopicType.ServerNetwork);
        }

        public void SaveRecording()
        {
            _playbackIOService.SavePlayback(_initialTick, _seed, _ticks, InitialSimulationState);
        }

        public void LoadPlayback(PlaybackFile playbackFile)
        {
            _seed = playbackFile.Seed;
            _ticks = playbackFile.Ticks;
            _initialTick = playbackFile.InitialTick;
            InitialSimulationState = playbackFile.InitialSimulationState;
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
