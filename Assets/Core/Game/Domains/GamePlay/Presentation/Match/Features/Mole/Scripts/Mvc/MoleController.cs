using System.Threading;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.StageCancellationToken;
using Core.Scripts.Services.AudioService;
using Core.Scripts.Utils;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.Mole.Scripts.Mvc
{
    public class MoleController
    {
        private readonly IStageCancellationTokenProvider _stageCancellationTokenProvider;
        private readonly IAudioService _audioService;

        private readonly MoleViewPool _moleViewPool;
        private readonly Transform _parent;
        private MoleView _moleView;
        private CancellationTokenSource _animationCancellationTokenSource;

        public Vector2 SpawnPointPosition { get; }

        public bool HasActiveMole { get; private set; }

        public MoleController(Vector2 spawnPointPosition, MoleViewPool moleViewPool, Transform parent, IStageCancellationTokenProvider stageCancellationTokenProvider, IAudioService audioService)
        {
            SpawnPointPosition = spawnPointPosition;
            _moleViewPool = moleViewPool;
            _parent = parent;
            _stageCancellationTokenProvider = stageCancellationTokenProvider;
            _audioService = audioService;
        }

        public void CreateView()
        {
            _moleView = _moleViewPool.Spawn();
            _moleView.transform.SetParent(_parent);
            _moleView.SetPosition(SpawnPointPosition);
            HasActiveMole = false;
            _moleView.ShowInHoleImmediately();
        }

        public void DestroyView()
        {
            CancelRunningAnimation();
            HasActiveMole = false;
            _moleView.Despawn();
        }

        public void SetHitState()
        {
            var animationCancellationToken = StartAnimation();
            HasActiveMole = false;
            _moleView.PlayHitAsync(animationCancellationToken).Forget();
        }

        public void SetInHoleState(bool playSoundFx = true)
        {
            var animationCancellationToken = StartAnimation();
            HasActiveMole = false;

            if (playSoundFx)
            {
                _audioService.PlayAudio(AudioClipType.MoleDespawned);
            }
            _moleView.PlayHideInHoleAsync(animationCancellationToken).Forget();
        }

        public void SetExpiringState(float shakeDurationSeconds)
        {
            var didMoleAlreadyFinishShaking = shakeDurationSeconds <= 0;
            if (didMoleAlreadyFinishShaking)
            {
                SetInHoleState();
                return;
            }

            ShakeThenHideAsync(shakeDurationSeconds).Forget();
        }

        public void SetEmergingFromHoleState(float shakeDurationSeconds, bool isGolden, byte remainingLives, byte maxLives)
        {
            HasActiveMole = true;
            _moleView.SetIsGolden(isGolden);

            var didMoleAlreadyFinishShaking = shakeDurationSeconds <= 0;
            if (didMoleAlreadyFinishShaking)
            {
                CancelRunningAnimation();
                _moleView.ShowOutsideHoleImmediately();
                _moleView.ShowHealthBar(remainingLives, maxLives);
                return;
            }

            ShakeHoleAndEmergeAsync(shakeDurationSeconds, remainingLives, maxLives).Forget();
        }

        public void SetGoldenMoleDamaged(byte remainingLives, byte maxLives)
        {
            _moleView.UpdateHealthBar(remainingLives, maxLives, _stageCancellationTokenProvider.CancellationTokenSource.Token);
        }

        private async Awaitable ShakeHoleAndEmergeAsync(float shakeDurationSeconds, byte remainingLives, byte maxLives)
        {
            var animationCancellationToken = StartAnimation();
            await _moleView.PlayHoleShakeAsync(shakeDurationSeconds, animationCancellationToken);
            _audioService.PlayAudio(AudioClipType.MoleSpawned);
            await _moleView.PlayEmergeFromHoleAsync(animationCancellationToken);
            _moleView.ShowHealthBar(remainingLives, maxLives);
        }

        private async Awaitable ShakeThenHideAsync(float shakeDurationSeconds)
        {
            var animationCancellationToken = StartAnimation();
            await _moleView.PlayShakeInPlaceAsync(shakeDurationSeconds, animationCancellationToken);
            SetInHoleState();
        }

        private CancellationToken StartAnimation()
        {
            CancelRunningAnimation();
            _animationCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_stageCancellationTokenProvider.CancellationTokenSource.Token);
            return _animationCancellationTokenSource.Token;
        }

        private void CancelRunningAnimation()
        {
            if (_animationCancellationTokenSource == null)
            {
                return;
            }

            _animationCancellationTokenSource.Cancel();
            _animationCancellationTokenSource.Dispose();
            _animationCancellationTokenSource = null;
        }
    }
}
