using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Presentation.Scripts.ScriptableObjects;
using Core.Scripts.Extensions;
using UnityEngine;
using Zenject;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.FrigidBlock.Scripts.Mvc
{
    public class FrigidBlocksControllers : IFrigidBlocksControllers
    {
        private readonly PresentationGamePlayConfig _gamePlayConfig;
        private readonly SharedGamePlayConfig _sharedGamePlayConfig;
        private readonly FrigidBlockPool _pool;
        private readonly Dictionary<ushort, FrigidBlockController> _controllers = new();
        private Transform _parentTransform;
        private Mesh _blockMesh;

        public FrigidBlocksControllers(FrigidBlockView prefab, DiContainer diContainer, PresentationGamePlayConfig gamePlayConfig, SharedGamePlayConfig sharedGamePlayConfig)
        {
            _gamePlayConfig = gamePlayConfig;
            _sharedGamePlayConfig = sharedGamePlayConfig;
            _pool = new FrigidBlockPool(prefab, diContainer);
        }

        public void InitEntryPoint()
        {
            _parentTransform = (new GameObject("FrigidBlocksParent")).transform;
            _pool.InitPool();
            // Build the block quad from its collider box (matching the physics SetAsBox) so it is rendered
            // from its collider like environment walls are. All blocks share this single mesh.
            _blockMesh = MeshUtils.CreateRectangleMesh(_sharedGamePlayConfig.FrigidBlockSize.ToNumericsVector2());
        }

        public void CreateFrigidBlock(ushort blockId, Vector2 position, Vector2 rotation)
        {
            if (_controllers.ContainsKey(blockId))
            {
                return;
            }

            var controller = new FrigidBlockController(blockId, _pool, _parentTransform);
            controller.CreateView(position, rotation.ToQuaternion(), _blockMesh);
            _controllers.Add(blockId, controller);
        }

        public void InterpolateFrigidBlockTransform(ushort blockId, Vector2 position, Quaternion rotation)
        {
            if (_controllers.TryGetValue(blockId, out var controller))
            {
                controller.InterpolateTransform(position, rotation, _gamePlayConfig.ExponentialDecay);
            }
        }

        public void DestroyFrigidBlock(ushort blockId)
        {
            if (_controllers.TryGetValue(blockId, out var controller))
            {
                controller.Destroy();
                _controllers.Remove(blockId);
            }
        }

        public void DestroyAll()
        {
            foreach (var controller in _controllers.Values)
            {
                controller.Destroy();
            }

            _controllers.Clear();
        }
    }
}
