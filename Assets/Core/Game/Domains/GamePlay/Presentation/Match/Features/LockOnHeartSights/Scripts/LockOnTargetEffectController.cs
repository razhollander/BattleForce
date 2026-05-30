using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.StageCancellationToken;
using Core.Scripts.Network;
using Core.Scripts.Utils;
using Core.Scripts.Utils.CustomCollections;
using CoreDomain.Scripts.Services.Logger.Base;
using UnityEngine;
using Zenject;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.LockOnHeartSights.Scripts
{
    public class LockOnTargetEffectController : ILockOnTargetEffectController
    {
        private readonly SharedGamePlayConfig _sharedGamePlayConfig;
        private readonly NetworkConfig _networkConfig;
        private readonly IStageCancellationTokenProvider _stageCancellationTokenProvider;
        private readonly LockOnTargetEffectPool _effectsPool;

        private readonly Dictionary<ushort, Dictionary<ushort, LockOnTargetEffectView>> _activeEffectsPerCaster = new Dictionary<ushort, Dictionary<ushort, LockOnTargetEffectView>>();

        public LockOnTargetEffectController(
            LockOnTargetEffectView prefab, DiContainer diContainer, SharedGamePlayConfig sharedGamePlayConfig, NetworkConfig networkConfig, IStageCancellationTokenProvider stageCancellationTokenProvider)
        {
            _sharedGamePlayConfig = sharedGamePlayConfig;
            _networkConfig = networkConfig;
            _stageCancellationTokenProvider = stageCancellationTokenProvider;
            _effectsPool = new LockOnTargetEffectPool(prefab, diContainer);
        }

        public void InitEntryPoint()
        {
            _effectsPool.InitPool();
        }

        public void AddPlayer(ushort casterPlayerId, FixedUnorderedList<ushort> casterTargetedEnemyIds)
        {
            _activeEffectsPerCaster[casterPlayerId] = new Dictionary<ushort, LockOnTargetEffectView>(_networkConfig.MaxCap.ConcurrentPlayers - 1);
            RefreshTargetEffectsOfCaster(casterPlayerId, casterTargetedEnemyIds);
        }

        public void RefreshTargetEffectsOfCaster(ushort casterPlayerId, FixedUnorderedList<ushort> playerIdsLockedOnTarget)
        {
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

                var newTargetEffectView = _effectsPool.Spawn();
                newTargetEffectView.Setup(_sharedGamePlayConfig.LockOnTargetDurationInSeconds);
                newTargetEffectView.PlayLockOnTargetAnimation(_stageCancellationTokenProvider.CancellationTokenSource.Token).Forget();
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

        public void DestroyAll()
        {
            foreach (var casterActiveEffects in _activeEffectsPerCaster.Values)
            {
                foreach (var effectView in casterActiveEffects.Values)
                {
                    effectView.Despawn();
                }
            }
            
            _activeEffectsPerCaster.Clear();
        }
    }
}
