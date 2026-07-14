using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Presentation.Scripts.ScriptableObjects;
using Core.Scripts.Extensions;
using Core.Scripts.Services.AudioService;
using CoreDomain.Scripts.Services.Logger.Base;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.CoolBGMusic.Scripts
{
    public class CoolBGMusicController : ICoolBGMusicController
    {
        private const int NO_LOOP_ID = -1;

        private readonly IAudioService _audioService;
        private readonly PresentationGamePlayConfig _gamePlayConfig;
        private readonly List<int> _shuffledOrder = new();

        private int _currentLoopId = NO_LOOP_ID;
        private int _nextOrderIndex;

        public CoolBGMusicController(IAudioService audioService, PresentationGamePlayConfig gamePlayConfig)
        {
            _audioService = audioService;
            _gamePlayConfig = gamePlayConfig;
        }

        public void InitEntryPoint()
        {
            _shuffledOrder.Clear();
            _nextOrderIndex = 0;

            var config = _gamePlayConfig.CoolBGMusicConfig;
            if (config == null || config.BackgroundMusics == null)
            {
                return;
            }

            for (var i = 0; i < config.BackgroundMusics.Count; i++)
            {
                _shuffledOrder.Add(i);
            }

            _shuffledOrder.Shuffle();
        }

        public bool TryPlayStageBackgroundMusic()
        {
            if (!_gamePlayConfig.CoolBGMusic)
            {
                return false;
            }

            if (_shuffledOrder.Count == 0)
            {
                LogService.LogError("CoolBGMusic is enabled but CoolBGMusicConfig has no background musics assigned");
                return false;
            }

            bool isCurrentLoopPlaying = _currentLoopId != NO_LOOP_ID;
            if (isCurrentLoopPlaying)
            {
                _audioService.StopLoopAudioById(_currentLoopId);
                _currentLoopId = NO_LOOP_ID;
            }

            var backgroundMusicIndex = _shuffledOrder[_nextOrderIndex];
            _nextOrderIndex = (_nextOrderIndex + 1) % _shuffledOrder.Count;

            var backgroundMusic = _gamePlayConfig.CoolBGMusicConfig.BackgroundMusics[backgroundMusicIndex];
            _currentLoopId = _audioService.PlayAudioLoopWithId(backgroundMusic.Clip, backgroundMusic.Volume);

            return true;
        }
    }
}
