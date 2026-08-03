using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.StageCancellationToken;
using Core.Scripts.Utils;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.Mole.Scripts.Mvc
{
    /// <summary>
    /// Owns the single mole that lives at one spawn point for the whole stage. The mole is never created or
    /// destroyed per server spawn, it only moves between its three states.
    /// </summary>
    public class MoleController
    {
        private readonly MolePool _molePool;
        private readonly Transform _parent;
        private readonly IStageCancellationTokenProvider _stageCancellationTokenProvider;
        private readonly float _hitStateDurationSeconds;
        private MoleView _moleView;

        public Vector2 SpawnPointPosition { get; }
        public MoleStateType StateType { get; private set; }

        public MoleController(Vector2 spawnPointPosition, MolePool molePool, Transform parent, IStageCancellationTokenProvider stageCancellationTokenProvider,
            float hitStateDurationSeconds)
        {
            SpawnPointPosition = spawnPointPosition;
            _molePool = molePool;
            _parent = parent;
            _stageCancellationTokenProvider = stageCancellationTokenProvider;
            _hitStateDurationSeconds = hitStateDurationSeconds;
        }

        public void CreateView()
        {
            _moleView = _molePool.Spawn();
            _moleView.transform.SetParent(_parent);
            _moleView.SetPosition(SpawnPointPosition);
            SetState(MoleStateType.InHole);
        }

        public void DestroyView()
        {
            _moleView.Despawn();
        }

        public void SetState(MoleStateType stateType)
        {
            StateType = stateType;
            _moleView.PlayState(stateType);
        }

        public void SetHitState()
        {
            SetState(MoleStateType.Hit);
            ReturnToHoleAfterHitAsync().Forget();
        }

        private async Awaitable ReturnToHoleAfterHitAsync()
        {
            await Awaitable.WaitForSecondsAsync(_hitStateDurationSeconds, _stageCancellationTokenProvider.CancellationTokenSource.Token);

            if (StateType == MoleStateType.Hit) // a fresh spawn during the hit animation wins over the delayed return
            {
                SetState(MoleStateType.InHole);
            }
        }
    }
}
