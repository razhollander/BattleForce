using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Presentation.Scripts.ScriptableObjects;
using Core.Scripts.Extensions;
using UnityEngine;
using Zenject;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.FrigidBlock.Scripts.Mvc
{
    public class FrigidBlocksControllers : IFrigidBlocksControllers
    {
        private const string PARENT_GAME_OBJECT_NAME = "FrigidBlocksParent";

        private readonly PresentationGamePlayConfig _gamePlayConfig;
        private readonly SharedGamePlayConfig _sharedGamePlayConfig;
        private readonly FrigidBlockPool _pool;
        private readonly Dictionary<ushort, FrigidBlockController> _controllers = new();
        private readonly FrigidBlockTrailViewControllersCache _trailViewControllersCache = new();
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
            _parentTransform = (new GameObject(PARENT_GAME_OBJECT_NAME)).transform;
            _pool.InitPool();
            _blockMesh = CreateSharedBlockMeshFromColliderSize();
        }

        public void InitExitPoint()
        {
            if (_parentTransform == null)
            {
                return;
            }

            DestroyAll();
            _trailViewControllersCache.DestroyCachedTrailMeshes();
            _pool.DisposePool();
            Object.Destroy(_blockMesh);
            _blockMesh = null;
            Object.Destroy(_parentTransform.gameObject);
            _parentTransform = null;
        }

        public void CreateFrigidBlock(ushort blockId, Vector2 position, Vector2 rotation)
        {
            if (_controllers.ContainsKey(blockId))
            {
                return;
            }

            var controller = new FrigidBlockController(blockId, _pool, _parentTransform, _trailViewControllersCache);
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

        private Mesh CreateSharedBlockMeshFromColliderSize()
        {
            return MeshUtils.CreateRectangleMesh(_sharedGamePlayConfig.FrigidBlockSize.ToNumericsVector2());
        }
    }
}
