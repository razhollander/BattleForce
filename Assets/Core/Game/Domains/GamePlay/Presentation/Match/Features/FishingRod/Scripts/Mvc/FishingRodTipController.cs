using Core.Game.Domains.GamePlay.Shared.Scripts.Enums;
using Core.Scripts.Services.AudioService;
using UnityEngine;
using CoreDomain.Scripts.Utils;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.FishingRod.Scripts.Mvc
{
    public class FishingRodTipController
    {
        private const string TIP_NAME = "FishingRodTip_";
        
        private readonly ushort _tipId;
        private readonly FishingRodTipPool _pool;
        private readonly Transform _parent;
        private FishingRodTipView _view;
        private int? _reelLoopAudioId;

        private readonly IAudioService _audioService;

        public FishingRodTipController(ushort tipId, FishingRodTipPool pool, Transform parent, IAudioService audioService)
        {
            _tipId = tipId;
            _pool = pool;
            _parent = parent;
            _audioService = audioService;
        }

        public void CreateView(Vector2 position, Quaternion rotation, Vector2 casterPosition, FishingRodTipPhase phase)
        {
            _view = _pool.Spawn();
            _view.name = TIP_NAME + _tipId;
            _view.transform.SetParent(_parent);
            _view.Setup(position, rotation, casterPosition);
            
            if (phase != FishingRodTipPhase.CaughtEnemy)
            {
                _reelLoopAudioId = _audioService.PlayAudioLoopWithId(AudioClipType.FishingRodReel);
            }
        }

        public void StopReelLoopAudio()
        {
            if (!_reelLoopAudioId.HasValue)
            {
                return;
            }

            _audioService.StopLoopAudioById(_reelLoopAudioId.Value);
            _reelLoopAudioId = null;
        }

        public void InterpolateTransform(Vector2 position, Quaternion rotation, Vector2 casterPosition, float decay)
        {
            var lerpedPosition = MathUtils.ExpDecay(_view.Transform.position, position, decay, Time.deltaTime);
            var lerpedRotation = MathUtils.ExpDecay(_view.Transform.rotation, rotation, decay, Time.deltaTime);
            _view.SetTransform(lerpedPosition, lerpedRotation, casterPosition);
        }

        public void Destroy()
        {
            StopReelLoopAudio();
            _view.Despawn();
        }
    }
}
