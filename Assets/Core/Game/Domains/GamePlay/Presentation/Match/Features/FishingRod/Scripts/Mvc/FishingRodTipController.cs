using Core.Game.Domains.GamePlay.Shared.Scripts.Enums;
using Core.Scripts.Services.AudioService;
using UnityEngine;
using CoreDomain.Scripts.Utils;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.FishingRod.Scripts.Mvc
{
    public class FishingRodTipController
    {
        private const string TipName = "FishingRodTip_";
        private readonly ushort _tipId;
        private readonly FishingRodTipPool _pool;
        private readonly Transform _parent;
        private readonly IAudioService _audioService;
        private FishingRodTipView _view;
        private int? _reelLoopAudioId;

        public ushort CasterPlayerId { get; private set; }

        public FishingRodTipController(ushort tipId, ushort casterPlayerId, FishingRodTipPool pool, Transform parent, IAudioService audioService)
        {
            _tipId = tipId;
            CasterPlayerId = casterPlayerId;
            _pool = pool;
            _parent = parent;
            _audioService = audioService;
        }

        public void CreateView(Vector2 position, Quaternion rotation, Vector2 casterPosition, FishingRodTipPhase phase)
        {
            _view = _pool.Spawn();
            _view.name = TipName + _tipId;
            _view.transform.SetParent(_parent);
            _view.Setup(position, rotation, casterPosition);

            // The reel loop plays while the rod is out reeling; once an enemy is caught it should be silent. It is
            // played with its own id so it can be stopped specifically, and it is stopped when the tip is destroyed
            // (talent deactivate, stage exit, or state resync) so it never lingers.
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
