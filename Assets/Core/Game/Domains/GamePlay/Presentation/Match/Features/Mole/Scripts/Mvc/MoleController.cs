using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.StageCancellationToken;
using Core.Scripts.Utils;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.Mole.Scripts.Mvc
{
    public class MoleController
    {
        private readonly MolePool _molePool;
        private readonly Transform _parent;
        private readonly IStageCancellationTokenProvider _stageCancellationTokenProvider;
        private MoleView _moleView;

        public ushort MoleId { get; }

        public MoleController(ushort moleId, MolePool molePool, Transform parent, IStageCancellationTokenProvider stageCancellationTokenProvider)
        {
            MoleId = moleId;
            _molePool = molePool;
            _parent = parent;
            _stageCancellationTokenProvider = stageCancellationTokenProvider;
        }

        public void CreateView(Vector2 position)
        {
            _moleView = _molePool.Spawn();
            _moleView.transform.SetParent(_parent);
            _moleView.SetPosition(position);
            _moleView.PlaySpawnAnimation();
        }

        public Vector2 GetPosition()
        {
            return _moleView.transform.position;
        }

        public void DestroyViewWithHitEffect()
        {
            _moleView.PlayHitAndDespawn(_stageCancellationTokenProvider.CancellationTokenSource.Token).Forget();
        }

        public void DestroyViewWithExpireEffect()
        {
            _moleView.PlayExpireAndDespawn(_stageCancellationTokenProvider.CancellationTokenSource.Token).Forget();
        }

        public void DestroyViewImmediately()
        {
            _moleView.Despawn();
        }
    }
}
