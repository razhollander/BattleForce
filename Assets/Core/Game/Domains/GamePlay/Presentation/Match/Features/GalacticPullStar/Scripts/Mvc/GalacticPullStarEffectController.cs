using System.Threading;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.GalacticPullStar.Scripts.Mvc
{
    public class GalacticPullStarEffectController
    {
        public readonly ushort FieldId;
        private readonly GalacticPullStarEffectPool _pool;
        private readonly Transform _starsParent;
        private GalacticPullStarEffectView _view;

        public GalacticPullStarEffectController(ushort fieldId, GalacticPullStarEffectPool pool, Transform starsParent)
        {
            FieldId = fieldId;
            _pool = pool;
            _starsParent = starsParent;
        }

        public void CreateView(Color outlineColor)
        {
            _view = _pool.Spawn();
            _view.name = "GalacticPullStarEffect_" + FieldId;
            _view.transform.SetParent(_starsParent, false);
            _view.Setup(outlineColor);
        }

        public async Awaitable SlideIn(float targetLocalX, CancellationTokenSource cancellationTokenSource)
        {
            await _view.SlideIn(targetLocalX, cancellationTokenSource);
        }

        public async Awaitable MoveToSlot(float targetLocalX, CancellationTokenSource cancellationTokenSource)
        {
            await _view.MoveToSlot(targetLocalX, cancellationTokenSource);
        }

        public async Awaitable SlideOutAndDestroy(CancellationTokenSource cancellationTokenSource)
        {
            await _view.SlideOut(cancellationTokenSource);
        }

        public void Destroy()
        {
            _view.Despawn();
        }
    }
}
