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

        public void CreateView(Color outlineColor, GalacticStarVisualData visualData)
        {
            _view = _pool.Spawn();
            _view.name = "GalacticPullStarEffect_" + FieldId;
            _view.transform.SetParent(_starsParent, false);
            _view.Setup(outlineColor, visualData);
        }

        public void SetSortingOrder(int order)
        {
            _view.SetSortingOrder(order);
        }

        public async Awaitable ScaleInAsync(float targetLocalY, CancellationToken cancellationToken)
        {
            await _view.ScaleInAsync(targetLocalY, cancellationToken);
        }

        public async Awaitable MoveToSlotAsync(float targetLocalY, CancellationToken cancellationToken)
        {
            await _view.MoveToSlotAsync(targetLocalY, cancellationToken);
        }

        public async Awaitable SlideOutAndDestroyAsync(CancellationToken cancellationToken)
        {
            await _view.SlideOutAsync(cancellationToken);
        }

        public void Destroy()
        {
            _view.Despawn();
        }
    }
}
