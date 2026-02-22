using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Presentation.Scripts.MVC.EnvironmentTeleportGate;
using UnityEngine;
using Zenject;

namespace Core.Game.Domains.GamePlay.Presentation.Scripts.MVC.EnvironmentTeleportGate
{
    public class MatchEnvironmentTeleportGateControllers
    {
        private readonly EnvironmentTeleportGateView _prefab;
        private readonly DiContainer _container;
        private readonly Transform _parent;
        private readonly List<EnvironmentTeleportGateController> _controllers = new List<EnvironmentTeleportGateController>();

        public MatchEnvironmentTeleportGateControllers(EnvironmentTeleportGateView prefab, DiContainer container)
        {
            _prefab = prefab;
            _container = container;
            _parent = new GameObject("EnvironmentTeleportGates").transform;
        }

        public EnvironmentTeleportGateController CreateGate(ushort pairId, bool isGateB, System.Numerics.Vector2 position, float rotation, Vector2 size, Color color)
        {
            var view = _container.InstantiatePrefabForComponent<EnvironmentTeleportGateView>(_prefab, _parent);
            var model = new EnvironmentTeleportGateModel(pairId, isGateB, position, rotation);
            var controller = new EnvironmentTeleportGateController(view, model);
            controller.Init(size, color);
            _controllers.Add(controller);
            return controller;
        }

        public EnvironmentTeleportGateController GetGate(ushort pairId, bool isGateB)
        {
            return _controllers.Find(c => c.PairId == pairId && c.IsGateB == isGateB);
        }

        public void DestroyAll()
        {
            // Implementation for destroying views if needed, though they are usually destroyed with the scene or parent
            // But since we instantiate them, we might want to clean up list
            _controllers.Clear();
            if (_parent != null)
            {
                Object.Destroy(_parent.gameObject);
            }
        }
    }
}
