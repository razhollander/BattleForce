using UnityEngine;
using CoreDomain.Scripts.Utils;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.Soul.Scripts.Mvc
{
    public class SoulGhostController
    {
        private const string GhostName = "SoulGhost_";
        private readonly ushort _ghostId;
        private readonly SoulGhostPool _pool;
        private readonly Transform _parent;
        private SoulGhostView _view;

        public ushort CasterPlayerId { get; private set; }

        public SoulGhostController(ushort ghostId, ushort casterPlayerId, SoulGhostPool pool, Transform parent)
        {
            _ghostId = ghostId;
            CasterPlayerId = casterPlayerId;
            _pool = pool;
            _parent = parent;
        }

        public void CreateView(Vector2 position, Quaternion rotation, Color teamColor)
        {
            _view = _pool.Spawn();
            _view.name = GhostName + _ghostId;
            _view.transform.SetParent(_parent);
            _view.Setup(position, rotation, teamColor);
        }

        public void InterpolateTransform(Vector2 position, Quaternion rotation, float decay)
        {
            var lerpedPosition = MathUtils.ExpDecay(_view.Transform.position, position, decay, Time.deltaTime);
            var lerpedRotation = MathUtils.ExpDecay(_view.Transform.rotation, rotation, decay, Time.deltaTime);
            _view.SetTransform(lerpedPosition, lerpedRotation);
        }

        public void Destroy()
        {
            _view.Despawn();
        }
    }
}
