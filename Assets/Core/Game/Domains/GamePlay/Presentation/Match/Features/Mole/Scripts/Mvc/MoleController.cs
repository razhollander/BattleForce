using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.StageCancellationToken;
using Core.Scripts.Services.AudioService;
using Core.Scripts.Utils;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.Mole.Scripts.Mvc
{
    /// <summary>
    /// Owns the single mole that lives at one spawn point for the whole stage. The mole is never created or
    /// destroyed per server spawn, it only moves between its states.
    /// </summary>
    public class MoleController
    {
        private const ushort NO_ACTIVE_MOLE_ID = ushort.MaxValue; // a mole id travels as a byte on the wire, so this can never collide with a real one

        private readonly IStageCancellationTokenProvider _stageCancellationTokenProvider;
        private readonly IAudioService _audioService;
        
        private readonly MolePool _molePool;
        private readonly Transform _parent;
        private MoleView _moleView;
        private MoleStateType _stateType;
        private byte _remainingLives;
        private byte _maxLives;

        public Vector2 SpawnPointPosition { get; }

        // The mole this spawn point currently belongs to. Net events of any older mole must not touch it anymore.
        public ushort ActiveMoleId { get; private set; } = NO_ACTIVE_MOLE_ID;

        public MoleController(Vector2 spawnPointPosition, MolePool molePool, Transform parent, IStageCancellationTokenProvider stageCancellationTokenProvider, IAudioService audioService)
        {
            SpawnPointPosition = spawnPointPosition;
            _molePool = molePool;
            _parent = parent;
            _stageCancellationTokenProvider = stageCancellationTokenProvider;
            _audioService = audioService;
        }

        public void CreateView()
        {
            _moleView = _molePool.Spawn();
            _moleView.transform.SetParent(_parent);
            _moleView.SetPosition(SpawnPointPosition);
            _stateType = MoleStateType.InHole;
            ActiveMoleId = NO_ACTIVE_MOLE_ID;
            _moleView.ShowInHoleImmediately();
        }

        public void DestroyView()
        {
            ActiveMoleId = NO_ACTIVE_MOLE_ID;
            _moleView.Despawn();
        }

        public void SetHitState()
        {
            _stateType = MoleStateType.Hit;
            ActiveMoleId = NO_ACTIVE_MOLE_ID;
            PlayHitAsync().Forget();
        }

        public void SetInHoleState(bool playSoundFx = true)
        {
            _stateType = MoleStateType.InHole;
            ActiveMoleId = NO_ACTIVE_MOLE_ID;

            if (playSoundFx)
            {
                _audioService.PlayAudio(AudioClipType.MoleDespawned);
            }
            _moleView.PlayHideInHoleAsync(_stageCancellationTokenProvider.CancellationTokenSource.Token).Forget();
        }

        // The mole's lifetime ended, it shakes in place while staying hittable and only hides once the shake is over. The
        // active mole id is kept for the whole shake so a hit can still land on it, only the hide at the end clears it.
        public void SetExpiringState(float shakeDurationSeconds)
        {
            if (shakeDurationSeconds <= 0) // a rejoining client can catch a mole that already finished its shake
            {
                SetInHoleState();
                return;
            }

            _stateType = MoleStateType.Expiring;
            ShakeThenHideAsync(shakeDurationSeconds).Forget();
        }

        // The mole stays hidden while its hole shakes, it only climbs out once the shake is over.
        public void SetEmergingFromHoleState(ushort moleId, float shakeDurationSeconds, bool isGolden, byte remainingLives, byte maxLives)
        {
            ActiveMoleId = moleId;
            _remainingLives = remainingLives;
            _maxLives = maxLives;
            _moleView.SetIsGolden(isGolden);

            if (shakeDurationSeconds <= 0) // a rejoining client can catch a mole that already finished shaking
            {
                _stateType = MoleStateType.OutsideHole;
                _moleView.ShowOutsideHoleImmediately();
                ShowHealthBar();
                return;
            }

            _stateType = MoleStateType.EmergingFromHole;
            ShakeHoleAndEmergeAsync(shakeDurationSeconds).Forget();
        }

        public void SetGoldenMoleDamaged(byte remainingLives, byte maxLives)
        {
            _remainingLives = remainingLives;
            _maxLives = maxLives;
            _moleView.UpdateHealthBar(remainingLives, maxLives, _stageCancellationTokenProvider.CancellationTokenSource.Token);
        }

        private async Awaitable ShakeHoleAndEmergeAsync(float shakeDurationSeconds)
        {
            await _moleView.PlayHoleShakeAsync(shakeDurationSeconds, _stageCancellationTokenProvider.CancellationTokenSource.Token);

            if (_stateType != MoleStateType.EmergingFromHole) // the mole may have expired while its hole was still shaking
            {
                return;
            }

            _stateType = MoleStateType.OutsideHole;
            _audioService.PlayAudio(AudioClipType.MoleSpawned);
            await _moleView.PlayEmergeFromHoleAsync(_stageCancellationTokenProvider.CancellationTokenSource.Token);

            if (_stateType != MoleStateType.OutsideHole) // the mole may have been hit or expired while it was still emerging
            {
                return;
            }

            ShowHealthBar();
        }

        private async Awaitable ShakeThenHideAsync(float shakeDurationSeconds)
        {
            await _moleView.PlayShakeInPlaceAsync(shakeDurationSeconds, _stageCancellationTokenProvider.CancellationTokenSource.Token);

            if (_stateType != MoleStateType.Expiring) // the mole may have been hit or respawned while it was still shaking
            {
                return;
            }

            SetInHoleState();
        }

        // Every mole shows a health bar once it is fully out of its hole (a normal mole has one life, a golden mole three).
        private void ShowHealthBar()
        {
            _moleView.ShowHealthBar(_remainingLives, _maxLives);
        }

        private async Awaitable PlayHitAsync()
        {
            await _moleView.PlayHitAsync(_stageCancellationTokenProvider.CancellationTokenSource.Token);

            if (_stateType == MoleStateType.Hit) // a fresh spawn during the hit animation wins over the delayed return
            {
                _stateType = MoleStateType.InHole;
            }
        }
    }
}
