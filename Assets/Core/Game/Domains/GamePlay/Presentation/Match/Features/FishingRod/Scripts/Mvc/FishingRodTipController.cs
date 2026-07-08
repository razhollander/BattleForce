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
        private FishingRodTipView _view;

        public ushort CasterPlayerId { get; private set; }

        public FishingRodTipController(ushort tipId, ushort casterPlayerId, FishingRodTipPool pool, Transform parent)
        {
            _tipId = tipId;
            CasterPlayerId = casterPlayerId;
            _pool = pool;
            _parent = parent;
        }

        public void CreateView(Vector2 position, Quaternion rotation, Vector2 casterPosition)
        {
            _view = _pool.Spawn();
            _view.name = TipName + _tipId;
            _view.transform.SetParent(_parent);
            _view.Setup(position, rotation, casterPosition);
        }

        public void InterpolateTransform(Vector2 position, Quaternion rotation, Vector2 casterPosition, float decay)
        {
            var lerpedPosition = MathUtils.ExpDecay(_view.Transform.position, position, decay, Time.deltaTime);
            var lerpedRotation = MathUtils.ExpDecay(_view.Transform.rotation, rotation, decay, Time.deltaTime);
            _view.SetTransform(lerpedPosition, lerpedRotation, casterPosition);
        }

        public void Destroy()
        {
            _view.Despawn();
        }
    }
}
