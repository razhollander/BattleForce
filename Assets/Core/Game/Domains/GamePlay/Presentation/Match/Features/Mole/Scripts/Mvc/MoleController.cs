using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.StageCancellationToken;
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

        private readonly MolePool _molePool;
        private readonly Transform _parent;
        private readonly IStageCancellationTokenProvider _stageCancellationTokenProvider;
        private MoleView _moleView;
        private MoleStateType _stateType;
        private bool _isGolden;
        private byte _remainingLives;
        private byte _maxLives;

        public Vector2 SpawnPointPosition { get; }

        // The mole this spawn point currently belongs to. Net events of any older mole must not touch it anymore.
        public ushort ActiveMoleId { get; private set; } = NO_ACTIVE_MOLE_ID;

        public MoleController(Vector2 spawnPointPosition, MolePool molePool, Transform parent, IStageCancellationTokenProvider stageCancellationTokenProvider)
        {
            SpawnPointPosition = spawnPointPosition;
            _molePool = molePool;
            _parent = parent;
            _stageCancellationTokenProvider = stageCancellationTokenProvider;
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

        public void SetInHoleState()
        {
            _stateType = MoleStateType.InHole;
            ActiveMoleId = NO_ACTIVE_MOLE_ID;
            _moleView.PlayHideInHoleAsync(_stageCancellationTokenProvider.CancellationTokenSource.Token).Forget();
        }

        // The mole stays hidden while its hole shakes, it only climbs out once the shake is over.
        public void SetEmergingFromHoleState(ushort moleId, float shakeDurationSeconds, bool isGolden, byte remainingLives, byte maxLives)
        {
            ActiveMoleId = moleId;
            _isGolden = isGolden;
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
            await _moleView.PlayEmergeFromHoleAsync(_stageCancellationTokenProvider.CancellationTokenSource.Token);

            if (_stateType != MoleStateType.OutsideHole) // the mole may have been hit or expired while it was still emerging
            {
                return;
            }

            ShowHealthBar();
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
