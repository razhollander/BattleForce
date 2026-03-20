using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Shared.Scripts.MatchInitData;
using Core.Scripts.Extensions;
using CoreDomain.Scripts.Services.Logger.Base;
using Newtonsoft.Json;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.Playback
{
    public class PlaybackIOService : IPlaybackIOService
    {
        private const string PLAYBACK_NAME_PREFIX = "playback_";
        private readonly SharedGamePlayConfig _sharedGamePlayConfig;
        private readonly string _playbacksFolder;
        
        public PlaybackIOService(SharedGamePlayConfig sharedGamePlayConfig)
        {
            _sharedGamePlayConfig = sharedGamePlayConfig;
            _playbacksFolder = Path.Combine(Application.streamingAssetsPath, "Playbacks");
            
            if (!Directory.Exists(_playbacksFolder))
            {
                Directory.CreateDirectory(_playbacksFolder);
            }
        }

        public List<string> GetAllPlaybackNames()
        {
            if (!Directory.Exists(_playbacksFolder))
            {
                return new List<string>();
            }

            return Directory.GetFiles(_playbacksFolder, PLAYBACK_NAME_PREFIX + "*.json")
                .OrderByDescending(File.GetCreationTime)
                .Select(Path.GetFileName)
                .ToList();
        }

        public bool TryGetPlayback(string playbackName, out PlaybackFile playbackFile)
        {
            var filePath = Path.Combine(_playbacksFolder, playbackName);
            playbackFile = default;
            
            if (!File.Exists(filePath))
            {
                LogService.LogError($"Playback file not found at {filePath}");
                return false;
            }

            var json = File.ReadAllText(filePath);

            try
            {
                playbackFile = JsonConvert.DeserializeObject<PlaybackFile>(json);
                return true;
            }
            catch (Exception e)
            {
                LogService.LogError($"Failed to deserialize playback file playbackName, Exception: {e}");
                return false;
            }
        }

        public void SavePlayback(int _initialTick, int _seed, Dictionary<int, PlaybackTickData> _ticks, EnterMatchPlayerData[] players)
        {
            try
            {
                var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
                var fileName = PLAYBACK_NAME_PREFIX+$"{timestamp}.json";
                var filePath = Path.Combine(_playbacksFolder, fileName);

                var fileData = new PlaybackFile
                {
                    InitialTick = _initialTick,
                    Seed = _seed,
                    Ticks = _ticks,
                    Players = players
                };

                string json = JsonConvert.SerializeObject(fileData);
                File.WriteAllText(filePath, json);
                LogService.LogTopic($"Saved Playback to {filePath}", LogTopicType.ServerNetwork);
                DeleteOldPlaybacks();
            }
            catch (Exception e)
            {
                LogService.LogError($"Failed to save playback: {e}");
            }
        }

        private void DeleteOldPlaybacks()
        {
            try
            {
                var files = Directory.GetFiles(_playbacksFolder, PLAYBACK_NAME_PREFIX+ "*.json")
                    .OrderBy(File.GetCreationTime)
                    .ToList();

                while (files.Count > _sharedGamePlayConfig.MaxSavedPlaybacks)
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
    }
}