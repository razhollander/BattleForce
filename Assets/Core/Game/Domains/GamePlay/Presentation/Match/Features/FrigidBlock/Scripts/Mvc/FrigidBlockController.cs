using CoreDomain.Scripts.Utils;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.FrigidBlock.Scripts.Mvc
{
    public class FrigidBlockController
    {
        private const string FrigidBlockName = "FrigidBlock_";

        private readonly ushort _blockId;
        private readonly FrigidBlockPool _pool;
        private readonly Transform _parent;
        private FrigidBlockView _view;

        public ushort BlockId => _blockId;

        public FrigidBlockController(ushort blockId, FrigidBlockPool pool, Transform parent)
        {
            _blockId = blockId;
            _pool = pool;
            _parent = parent;
        }

        public void CreateView(Vector2 position, Quaternion rotation, Mesh mesh)
        {
            _view = _pool.Spawn();
            _view.name = FrigidBlockName + _blockId;
            _view.transform.SetParent(_parent);
            _view.SetMesh(mesh);
            _view.SetTransform(position, rotation);
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
