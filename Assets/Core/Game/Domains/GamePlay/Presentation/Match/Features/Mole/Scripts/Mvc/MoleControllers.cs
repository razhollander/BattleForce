using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.StageCancellationToken;
using Core.Game.Domains.GamePlay.Presentation.Scripts.ScriptableObjects;
using CoreDomain.Scripts.Services.Logger.Base;
using UnityEngine;
using Zenject;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.Mole.Scripts.Mvc
{
    /// <summary>
    /// One mole lives at every authored spawn point for the whole stage. A server spawn only pops the mole at the
    /// matching spawn point out of its hole, so moles are never created or destroyed mid stage.
    /// </summary>
    public class MoleControllers : IMoleControllers
    {
        private readonly MolePool _pool;
        private readonly PresentationGamePlayConfig _gamePlayConfig;
        private readonly IStageCancellationTokenProvider _stageCancellationTokenProvider;
        private readonly List<MoleController> _controllers = new List<MoleController>();
        private readonly Dictionary<ushort, MoleController> _controllerPerOutsideHoleMoleId = new Dictionary<ushort, MoleController>();
        private Transform _parent;

        public MoleControllers(MoleView moleViewPrefab, DiContainer diContainer, PresentationGamePlayConfig gamePlayConfig,
            IStageCancellationTokenProvider stageCancellationTokenProvider)
        {
            _pool = new MolePool(moleViewPrefab, diContainer);
            _gamePlayConfig = gamePlayConfig;
            _stageCancellationTokenProvider = stageCancellationTokenProvider;
        }

        public void InitEntryPoint()
        {
            _parent = (new GameObject("MolesParent")).transform;
            _pool.InitPool();
        }

        public void CreateMoleAtSpawnPoint(Vector2 spawnPointPosition)
        {
            var controller = new MoleController(spawnPointPosition, _pool, _parent, _stageCancellationTokenProvider, _gamePlayConfig.MoleHitStateDurationSeconds);
            controller.CreateView();
            _controllers.Add(controller);
        }

        public void SetMoleOutsideHole(ushort moleId, Vector2 position)
        {
            var controller = GetControllerNearestTo(position);

            if (controller == null)
            {
                LogService.LogError($"No mole spawn point to pop mole {moleId} out of!");
                return;
            }

            _controllerPerOutsideHoleMoleId[moleId] = controller;
            controller.SetState(MoleStateType.OutsideHole);
        }

        public void SetMoleHit(ushort moleId)
        {
            if (!TryTakeOutsideHoleController(moleId, out var controller))
            {
                return;
            }

            controller.SetHitState();
        }

        public void SetMoleInHole(ushort moleId)
        {
            if (!TryTakeOutsideHoleController(moleId, out var controller))
            {
                return;
            }

            controller.SetState(MoleStateType.InHole);
        }

        public Vector2 GetMolePosition(ushort moleId)
        {
            return _controllerPerOutsideHoleMoleId.TryGetValue(moleId, out var controller) ? controller.SpawnPointPosition : Vector2.zero;
        }

        public void DestroyAll()
        {
            foreach (var controller in _controllers)
            {
                controller.DestroyView();
            }

            _controllers.Clear();
            _controllerPerOutsideHoleMoleId.Clear();
        }

        private bool TryTakeOutsideHoleController(ushort moleId, out MoleController controller)
        {
            if (!_controllerPerOutsideHoleMoleId.TryGetValue(moleId, out controller))
            {
                LogService.LogError($"No mole outside its hole with id {moleId}!");
                return false;
            }

            _controllerPerOutsideHoleMoleId.Remove(moleId);
            return true;
        }

        // The spawn position is quantized on the wire, so the mole belongs to the closest authored spawn point.
        private MoleController GetControllerNearestTo(Vector2 position)
        {
            MoleController nearestController = null;
            var nearestDistanceSquared = float.MaxValue;

            foreach (var controller in _controllers)
            {
                var distanceSquared = (controller.SpawnPointPosition - position).sqrMagnitude;

                if (distanceSquared < nearestDistanceSquared)
                {
                    nearestDistanceSquared = distanceSquared;
                    nearestController = controller;
                }
            }

            return nearestController;
        }
    }
}
