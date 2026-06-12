using System.Collections.Generic;
using System.Linq;
using Core.Game.Domains.GamePlay.Shared.Scripts;
using Core.Game.Domains.GamePlay.Shared.Scripts.MatchInitData;
using Core.Game.Domains.GamePlay.Shared.Scripts.Playback;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager.TickHandlers.PacketsObservers;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Services.GamePlayConfig;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Services.TickService;
using Core.Scripts.Extensions;
using CoreDomain.Scripts.Services.Logger.Base;
using LiteNetLib;
using Newtonsoft.Json;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.Playback
{
    public class PlaybackRecorderService : IPlaybackRecorderService, IRawPacketsObserver
    {
        private readonly ITickService _tickService;
        private readonly IServerNetworkManager _networkManager;
        private readonly IPlaybackIOService _playbackIOService;

        private readonly ISimulationGamePlayConfigService _simulationGamePlayConfigService;
        private Dictionary<int, PlaybackTickData> _ticks = new Dictionary<int, PlaybackTickData>();
        private int _seed;
        private int _initialTick;
        private EnterMatchPlayerData[] _players;

        public EnterMatchPlayerData[] Players => _players;
        public int Seed => _seed;
        public int InitialTick => _initialTick;
        public bool IsPlaybackEnabled { get; private set; }

        public PlaybackRecorderService(ITickService tickService, IServerNetworkManager networkManager, IPlaybackIOService playbackIOService,
            ISimulationGamePlayConfigService simulationGamePlayConfigService)
        {
            _tickService = tickService;
            _networkManager = networkManager;
            _playbackIOService = playbackIOService;

            _simulationGamePlayConfigService = simulationGamePlayConfigService;
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

        public void StartRecording(int seed, EnterMatchPlayerData[] players)
        {
            _seed = seed;
            _players = players;
            _ticks.Clear();
            _initialTick = _tickService.CurrentTick;
            _networkManager.RegisterPacketsObserver(this);
            LogService.LogTopic($"Started Recording Playback. Seed: {seed}", LogTopicType.ServerNetwork);
        }

        private void SaveRecording()
        {
            _playbackIOService.SavePlayback(_initialTick, _seed, _ticks, _players, JsonConvert.SerializeObject(_simulationGamePlayConfigService.GamePlayConfig));
        }

        public void LoadPlayback(PlaybackFile playbackFile)
        {
            _simulationGamePlayConfigService.OverrideGamePlayConfig(playbackFile.SimulationConfigJson);
            _seed = playbackFile.Seed;
            _ticks = playbackFile.Ticks;
            _initialTick = playbackFile.InitialTick;
            _players = playbackFile.Players;
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
            
            var clientId = (long)peer.Tag;
            RecordPacket(clientId, packetBytes);
        }
        
        private void RecordPacket(long clientId, byte[] data)
        {
            var serverTick = _tickService.CurrentTick;
            if (!_ticks.TryGetValue(serverTick, out var tickData))
            {
                tickData = new PlaybackTickData { Tick = serverTick };
                _ticks[serverTick] = tickData;
            }

            tickData.Packets.Add(new RecordedPacket
            {
                ClientId = clientId,
                Data = data
            });
        }
    }
}
