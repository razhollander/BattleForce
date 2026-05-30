using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.Player.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.DataService;
using Core.Scripts.Network;
using Core.Scripts.Utils.CustomCollections;
using CoreDomain.Scripts.Services.Logger.Base;
using CoreDomain.Scripts.Services.UpdateService;
using UnityEngine;
using Zenject;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.LockOnHeartSights.Scripts
{
    public class LockOnTargetEffectController : ILockOnTargetEffectController
    {
        private readonly SharedGamePlayConfig _sharedGamePlayConfig;
        private readonly NetworkConfig _networkConfig;
        private readonly LockOnTargetEffectPool _pool;

        private readonly Dictionary<ushort, Dictionary<ushort, LockOnTargetEffectView>> _activeEffectsPerCaster = new Dictionary<ushort, Dictionary<ushort, LockOnTargetEffectView>>();

        public LockOnTargetEffectController(
            LockOnTargetEffectView prefab, DiContainer diContainer, SharedGamePlayConfig sharedGamePlayConfig, NetworkConfig networkConfig)
        {
            _sharedGamePlayConfig = sharedGamePlayConfig;
            _networkConfig = networkConfig;
            _pool = new LockOnTargetEffectPool(prefab, diContainer);
        }

        public void InitEntryPoint()
        {
            _pool.InitPool();
        }

        public void RefreshTargetEffectsOfCaster(ushort casterPlayerId, FixedUnorderedList<ushort> playerIdsLockedOnTarget)
        {
            if (!_activeEffectsPerCaster.ContainsKey(casterPlayerId))
            {
                _activeEffectsPerCaster[casterPlayerId] = new Dictionary<ushort, LockOnTargetEffectView>(_networkConfig.MaxCap.ConcurrentPlayers - 1);
            }

            var casterActiveEffects = _activeEffectsPerCaster[casterPlayerId];

            var enemieIdsToRemove = new List<ushort>();
            foreach (var enemyId in casterActiveEffects.Keys)
            {
                if (!playerIdsLockedOnTarget.Contains(enemyId))
                {
                    enemieIdsToRemove.Add(enemyId);
                }
            }

            foreach (var enemyId in enemieIdsToRemove)
            {
                casterActiveEffects[enemyId].Despawn();
                casterActiveEffects.Remove(enemyId);
            }

            foreach (var enemyId in playerIdsLockedOnTarget.AsSpan())
            {
                if (casterActiveEffects.ContainsKey(enemyId))
                {
                    continue;
                }

                var newTargetEffectView = _pool.Spawn();
                newTargetEffectView.Setup(_sharedGamePlayConfig.LockOnTargetDurationInSeconds);
                casterActiveEffects[enemyId] = newTargetEffectView;
            }
        }

        public void UpdateTargetsPositionOnPlayer(ushort casterPlayerId, ushort targetPlayerId, Vector2 startPoint, Vector2 endPoint)
        {
            if (!_activeEffectsPerCaster.TryGetValue(casterPlayerId, out var casterActiveEffects))
            {
                LogService.LogError($"No effects for caster player id: {casterPlayerId}");
                return;
            }

            if (!casterActiveEffects.TryGetValue(targetPlayerId, out var effectView))
            {
                LogService.LogError($"No effect for caster player id: {casterPlayerId} on target player id: {targetPlayerId}");
                return;
            }
            
            effectView.UpdatePosition(startPoint, endPoint, endPoint);
        }
    }
}
