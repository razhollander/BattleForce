using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.StageCancellationToken;
using CoreDomain.Scripts.Services.Logger.Base;
using UnityEngine;
using Zenject;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.Mole.Scripts.Mvc
{
    /// <summary>
    /// One mole lives at every authored spawn point for the whole stage. A server spawn only shakes the hole of the
    /// mole at the matching spawn point and then pops it out, so moles are never created or destroyed mid stage.
    /// </summary>
    public class MoleControllers : IMoleControllers
    {
        private readonly MolePool _pool;
        private readonly IStageCancellationTokenProvider _stageCancellationTokenProvider;
        private readonly List<MoleController> _controllers = new List<MoleController>();
        private readonly Dictionary<ushort, MoleController> _controllerPerActiveMoleId = new Dictionary<ushort, MoleController>();
        private Transform _parent;

        public MoleControllers(MoleView moleViewPrefab, DiContainer diContainer, IStageCancellationTokenProvider stageCancellationTokenProvider)
        {
            _pool = new MolePool(moleViewPrefab, diContainer);
            _stageCancellationTokenProvider = stageCancellationTokenProvider;
        }

        public void InitEntryPoint()
        {
            _parent = (new GameObject("MolesParent")).transform;
            _pool.InitPool();
        }

        public void CreateMoleAtSpawnPoint(Vector2 spawnPointPosition)
        {
            var controller = new MoleController(spawnPointPosition, _pool, _parent, _stageCancellationTokenProvider);
            controller.CreateView();
            _controllers.Add(controller);
        }

        public void SetMoleEmergingFromHole(ushort moleId, Vector2 position, float shakeDurationSeconds, bool isGolden, byte remainingLives, byte maxLives)
        {
            var controller = GetControllerNearestTo(position);

            if (controller == null)
            {
                LogService.LogError($"No mole spawn point to pop mole {moleId} out of!");
                return;
            }

            _controllerPerActiveMoleId[moleId] = controller;
            controller.SetEmergingFromHoleState(moleId, shakeDurationSeconds, isGolden, remainingLives, maxLives);
        }

        // A damaged golden mole is still active, so unlike a hit or expiry this must not drop it from the active lookup.
        public void SetGoldenMoleDamaged(ushort moleId, byte remainingLives, byte maxLives)
        {
            if (!_controllerPerActiveMoleId.TryGetValue(moleId, out var controller))
            {
                LogService.LogError($"No active golden mole with id {moleId}!");
                return;
            }

            if (controller.ActiveMoleId != moleId)
            {
                return;
            }

            controller.SetGoldenMoleDamaged(remainingLives, maxLives);
        }

        public void SetMoleHit(ushort moleId)
        {
            if (!TryTakeActiveMoleController(moleId, out var controller))
            {
                return;
            }

            controller.SetHitState();
        }

        public void SetMoleInHole(ushort moleId)
        {
            if (!TryTakeActiveMoleController(moleId, out var controller))
            {
                return;
            }

            controller.SetInHoleState();
        }

        public bool TryGetMolePosition(ushort moleId, out Vector2 position)
        {
            if (!_controllerPerActiveMoleId.TryGetValue(moleId, out var controller) || controller.ActiveMoleId != moleId)
            {
                position = Vector2.zero;
                return false;
            }

            position = controller.SpawnPointPosition;
            return true;
        }

        public void DestroyAll()
        {
            foreach (var controller in _controllers)
            {
                controller.DestroyView();
            }

            _controllers.Clear();
            _controllerPerActiveMoleId.Clear();
        }

        // Net events of the different mole types are drained one type at a time, so a spawn of the mole that reused this
        // spawn point can be handled before the hit or expiry of the mole it replaced. Such a stale event is dropped here.
        private bool TryTakeActiveMoleController(ushort moleId, out MoleController controller)
        {
            if (!_controllerPerActiveMoleId.TryGetValue(moleId, out controller))
            {
                LogService.LogError($"No active mole with id {moleId}!");
                return false;
            }

            _controllerPerActiveMoleId.Remove(moleId);
            return controller.ActiveMoleId == moleId;
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
