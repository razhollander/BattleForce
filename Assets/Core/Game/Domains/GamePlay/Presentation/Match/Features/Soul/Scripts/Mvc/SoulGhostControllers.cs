using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Presentation.Scripts.ScriptableObjects;
using Core.Scripts.Mvc.WorldCamera;
using UnityEngine;
using Zenject;
using Core.Scripts.Extensions;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.Soul.Scripts.Mvc
{
    public class SoulGhostControllers : ISoulGhostControllers
    {
        private readonly PresentationGamePlayConfig _gamePlayConfig;
        private readonly IWorldCameraController _worldCameraController;
        private readonly SoulGhostPool _pool;
        private readonly Dictionary<ushort, SoulGhostController> _controllers = new();
        private Transform _parentTransform;

        public SoulGhostControllers(SoulGhostView prefab, DiContainer diContainer, PresentationGamePlayConfig gamePlayConfig, IWorldCameraController worldCameraController)
        {
            _gamePlayConfig = gamePlayConfig;
            _worldCameraController = worldCameraController;
            _pool = new SoulGhostPool(prefab, diContainer);
        }

        public void InitEntryPoint()
        {
            _parentTransform = (new GameObject("SoulGhostsParent")).transform;
            _pool.InitPool();
        }

        public void CreateSoulGhost(ushort ghostId, ushort casterPlayerId, ushort teamId, Vector2 position, Vector2 rotation)
        {
            var teamColor = _gamePlayConfig.ColorPerTeamId[teamId];
            var controller = new SoulGhostController(ghostId, casterPlayerId, _pool, _parentTransform);
            controller.CreateView(position, rotation.ToQuaternion(), teamColor);
            _controllers[ghostId] = controller;
            _worldCameraController.AddFollowTarget(controller.Transform);
        }

        public void InterpolateSoulGhostTransform(ushort ghostId, Vector2 position, Quaternion rotation)
        {
            if (_controllers.TryGetValue(ghostId, out var controller))
            {
                controller.InterpolateTransform(position, rotation, _gamePlayConfig.ExponentialDecay);
            }
        }

        public void DestroySoulGhost(ushort ghostId)
        {
            if (_controllers.TryGetValue(ghostId, out var controller))
            {
                _worldCameraController.RemoveFollowTarget(controller.Transform);
                controller.Destroy();
                _controllers.Remove(ghostId);
            }
            else
            {
                Debug.LogError($"SoulGhostController with id {ghostId} not found");
            }
        }

        public void DestroyAll()
        {
            foreach (var controller in _controllers.Values)
            {
                _worldCameraController.RemoveFollowTarget(controller.Transform);
                controller.Destroy();
            }
            _controllers.Clear();
        }
    }
}
