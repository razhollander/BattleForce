using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.Player.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.DataService;
using CoreDomain.Scripts.Services.UpdateService;
using UnityEngine;
using Zenject;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.LockOnHeartSights.Scripts
{
    public class LockOnTargetEffectController : ILockOnTargetEffectController, IGUIUpdatable
    {
        private readonly IMatchDataService _matchDataService;
        private readonly IMatchPlayerControllers _matchPlayerControllers;
        private readonly IUpdateSubscriptionService _updateSubscriptionService;
        private readonly LockOnTargetEffectPool _pool;

        private readonly Dictionary<ushort, Dictionary<ushort, LockOnTargetEffectView>> _activeEffects = new Dictionary<ushort, Dictionary<ushort, LockOnTargetEffectView>>();

        public LockOnTargetEffectController(
            IMatchDataService matchDataService,
            IMatchPlayerControllers matchPlayerControllers,
            IUpdateSubscriptionService updateSubscriptionService,
            LockOnTargetEffectView prefab,
            DiContainer diContainer)
        {
            _matchDataService = matchDataService;
            _matchPlayerControllers = matchPlayerControllers;
            _updateSubscriptionService = updateSubscriptionService;
            _pool = new LockOnTargetEffectPool(prefab, diContainer);
        }

        public void InitEntryPoint()
        {
            _pool.InitPool();
            _updateSubscriptionService.RegisterUpdatable(this);
        }

        public void InitExitPoint()
        {
            foreach (var casterEffects in _activeEffects.Values)
            {
                foreach (var effectView in casterEffects.Values)
                {
                    effectView.Despawn();
                }
            }
            _activeEffects.Clear();

            _updateSubscriptionService.UnregisterUpdatable(this);
        }

        public void UpdateEffects()
        {
            foreach (var playerModel in _matchDataService.Players)
            {
                var casterId = playerModel.PlayerId;

                if (!_activeEffects.ContainsKey(casterId))
                {
                    _activeEffects[casterId] = new Dictionary<ushort, LockOnTargetEffectView>();
                }

                var casterActiveEffects = _activeEffects[casterId];
                var targetedEnemyIds = playerModel.Spaceship.TargetedEnemyIds;

                var toRemove = new List<ushort>();
                foreach (var enemyId in casterActiveEffects.Keys)
                {
                    if (!targetedEnemyIds.Contains(enemyId))
                    {
                        toRemove.Add(enemyId);
                    }
                }

                foreach (var enemyId in toRemove)
                {
                    casterActiveEffects[enemyId].Despawn();
                    casterActiveEffects.Remove(enemyId);
                }

                foreach (var enemyId in targetedEnemyIds)
                {
                    if (!casterActiveEffects.ContainsKey(enemyId))
                    {
                        casterActiveEffects[enemyId] = _pool.Spawn();
                    }
                }
            }

            var disconnectedPlayers = new List<ushort>();
            foreach (var casterId in _activeEffects.Keys)
            {
                if (_matchDataService.GetPlayer(casterId) == null)
                {
                    disconnectedPlayers.Add(casterId);
                }
            }

            foreach (var casterId in disconnectedPlayers)
            {
                foreach (var effectView in _activeEffects[casterId].Values)
                {
                    effectView.Despawn();
                }
                _activeEffects.Remove(casterId);
            }
        }

        public void UpdateTargetsPositionOnPlayer(UnityEngine.Vector3 playerHeartPosition)
        {
            // Functionally handled via ManagedUpdate looping through _activeEffects
            // Implemented as per request
        }

        public void ManagedUpdate()
        {
            foreach (var kvp in _activeEffects)
            {
                var casterId = kvp.Key;
                var casterTransform = _matchPlayerControllers.GetPlayerHeadTransform(casterId);
                if (casterTransform == null) continue;

                var casterPosition = casterTransform.position;

                foreach (var innerKvp in kvp.Value)
                {
                    var enemyId = innerKvp.Key;
                    var effectView = innerKvp.Value;

                    var enemyHeartTransform = _matchPlayerControllers.GetPlayerHeartTransform(enemyId);
                    if (enemyHeartTransform != null)
                    {
                        var enemyHeartPosition = enemyHeartTransform.position;
                        effectView.UpdatePositions(casterPosition, enemyHeartPosition);
                    }
                }
            }
        }

    }
}
