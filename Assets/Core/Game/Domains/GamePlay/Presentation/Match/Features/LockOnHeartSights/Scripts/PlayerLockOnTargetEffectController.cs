using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.StageCancellationToken;
using Core.Scripts.Network;
using Core.Scripts.Utils;
using Core.Scripts.Utils.CustomCollections;
using CoreDomain.Scripts.Services.Logger.Base;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.LockOnHeartSights.Scripts
{
    public class PlayerLockOnTargetEffectController
    {
        private readonly SharedGamePlayConfig _sharedGamePlayConfig;
        private readonly IStageCancellationTokenProvider _stageCancellationTokenProvider;
        private readonly LockOnTargetEffectPool _effectsPool;

        private Dictionary<ushort, LockOnTargetEffectView> _activeEffectsPerEnemy;
        private ushort _casterPlayerId;

        public PlayerLockOnTargetEffectController(ushort casterPlayerId,FixedUnorderedList<ushort> casterTargetedEnemyIds, LockOnTargetEffectPool effectsPool,
            SharedGamePlayConfig sharedGamePlayConfig, NetworkConfig networkConfig, IStageCancellationTokenProvider stageCancellationTokenProvider)
        {
            _casterPlayerId = casterPlayerId;
            _sharedGamePlayConfig = sharedGamePlayConfig;
            _stageCancellationTokenProvider = stageCancellationTokenProvider;
            _effectsPool = effectsPool;
            _activeEffectsPerEnemy = new Dictionary<ushort, LockOnTargetEffectView>(networkConfig.MaxCap.ConcurrentPlayers - 1);
            RefreshTargetEffectsOfCaster(casterTargetedEnemyIds);
        }
        
        public void RefreshTargetEffectsOfCaster(FixedUnorderedList<ushort> playerIdsLockedOnTarget)
        {
            var enemieIdsToRemove = new List<ushort>();
            foreach (var enemyId in _activeEffectsPerEnemy.Keys)
            {
                if (!playerIdsLockedOnTarget.Contains(enemyId))
                {
                    enemieIdsToRemove.Add(enemyId);
                }
            }

            foreach (var enemyId in enemieIdsToRemove)
            {
                _activeEffectsPerEnemy[enemyId].Despawn();
                _activeEffectsPerEnemy.Remove(enemyId);
            }

            foreach (var enemyId in playerIdsLockedOnTarget.AsSpan())
            {
                if (_activeEffectsPerEnemy.ContainsKey(enemyId))
                {
                    continue;
                }

                var newTargetEffectView = _effectsPool.Spawn();
                newTargetEffectView.Setup(_sharedGamePlayConfig.LockOnTargetDurationInSeconds);
                newTargetEffectView.PlayLockOnTargetAnimationLooped(_stageCancellationTokenProvider.CancellationTokenSource.Token).Forget();
                _activeEffectsPerEnemy[enemyId] = newTargetEffectView;
            }
        }

        public void UpdateTargetsPositionOnPlayer(ushort targetPlayerId, Vector2 startPoint, Vector2 endPoint)
        {
            if (!_activeEffectsPerEnemy.TryGetValue(targetPlayerId, out var effectView))
            {
                LogService.LogError($"No effect for caster player id: {_casterPlayerId} on target player id: {targetPlayerId}");
                return;
            }
            
            effectView.UpdatePosition(startPoint, endPoint, endPoint);
        }
        
        public void DestroyAll()
        {
            foreach (var effectView in _activeEffectsPerEnemy.Values)
            {
                effectView.Despawn();
            }
            
            _activeEffectsPerEnemy.Clear();
        }
    }
}