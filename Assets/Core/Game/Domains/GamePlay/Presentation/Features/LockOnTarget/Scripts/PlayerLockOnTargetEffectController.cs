using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Scripts.Extensions.Linq;
using Core.Scripts.Network;
using Core.Scripts.Utils;
using Core.Scripts.Utils.CustomCollections;
using CoreDomain.Scripts.Services.Logger.Base;
using CoreDomain.Scripts.Services.StateMachineService;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Features.LockOnTarget
{
    public class PlayerLockOnTargetEffectController
    {
        private readonly SharedGamePlayConfig _sharedGamePlayConfig;
        private readonly IStateMachineService _stateMachineService;
        private readonly LockOnTargetEffectPool _effectsPool;

        private readonly Dictionary<ushort, ActiveTargetEffect> _activeEffectsPerEnemy;
        private readonly List<ushort> _cachedEnemyIdsToRemove;
        private readonly ushort _casterPlayerId;

        public PlayerLockOnTargetEffectController(ushort casterPlayerId, LockOnTargetEffectPool effectsPool,
            SharedGamePlayConfig sharedGamePlayConfig, NetworkConfig networkConfig, IStateMachineService stateMachineService)
        {
            _casterPlayerId = casterPlayerId;
            _sharedGamePlayConfig = sharedGamePlayConfig;
            _stateMachineService = stateMachineService;
            _effectsPool = effectsPool;
            _activeEffectsPerEnemy = new Dictionary<ushort, ActiveTargetEffect>(networkConfig.MaxCap.ConcurrentPlayers - 1);
            _cachedEnemyIdsToRemove = new List<ushort>(networkConfig.MaxCap.ConcurrentPlayers - 1);
        }

        public void RefreshTargetEffectsOfCaster(FixedUnorderedList<PlayerOnTargetS2C> playerIdsLockedOnTarget)
        {
            DespawnTargetsWhichArentShownAnymore(playerIdsLockedOnTarget);

            foreach (var target in playerIdsLockedOnTarget.AsSpan())
            {
                UpdateLockOnTargetEffectForTarget(target);
            }
        }

        private void UpdateLockOnTargetEffectForTarget(PlayerOnTargetS2C target)
        {
            var enemyId = target.PlayerTargetId;
            var isShootable = target.IsLockOnTargetShootable;

            if (!_activeEffectsPerEnemy.TryGetValue(enemyId, out var activeEffect))
            {
                var newTargetEffectView = _effectsPool.Spawn();
                newTargetEffectView.Setup(_sharedGamePlayConfig.LockOnTargetDurationInSeconds);
                PlayAnimationForState(newTargetEffectView, isShootable);
                _activeEffectsPerEnemy[enemyId] = new ActiveTargetEffect(newTargetEffectView, isShootable);
                return;
            }

            var didShootableChange = activeEffect.IsShootable != isShootable;
            if (didShootableChange)
            {
                PlayAnimationForState(activeEffect.View, isShootable);
                _activeEffectsPerEnemy[enemyId] = new ActiveTargetEffect(activeEffect.View, isShootable);
            }
        }

        private void DespawnTargetsWhichArentShownAnymore(FixedUnorderedList<PlayerOnTargetS2C> playerIdsLockedOnTarget)
        {
            _cachedEnemyIdsToRemove.Clear();
            foreach (var enemyId in _activeEffectsPerEnemy.Keys)
            {
                if (!playerIdsLockedOnTarget.ContainsWithId(enemyId))
                {
                    _cachedEnemyIdsToRemove.Add(enemyId);
                }
            }

            foreach (var enemyId in _cachedEnemyIdsToRemove)
            {
                _activeEffectsPerEnemy[enemyId].View.Despawn();
                _activeEffectsPerEnemy.Remove(enemyId);
            }
        }

        private void PlayAnimationForState(LockOnTargetEffectView view, bool isShootable)
        {
            var cancellationToken = _stateMachineService.CurrentState().CancellationTokenSource.Token;
            if (isShootable)
            {
                view.PlayLockOnTargetShootableAnimation(cancellationToken).Forget();
            }
            else
            {
                view.PlayLockOnTargetAnimation(cancellationToken).Forget();
            }
        }

        public void UpdateTargetsPositionOnPlayer(ushort targetPlayerId, Vector2 startPoint, Vector2 endPoint)
        {
            if (!_activeEffectsPerEnemy.TryGetValue(targetPlayerId, out var activeEffect))
            {
                LogService.LogError($"No effect for caster player id: {_casterPlayerId} on target player id: {targetPlayerId}");
                return;
            }

            activeEffect.View.UpdatePosition(startPoint, endPoint, endPoint);
        }

        public void DestroyAll()
        {
            foreach (var activeEffect in _activeEffectsPerEnemy.Values)
            {
                activeEffect.View.Despawn();
            }

            _activeEffectsPerEnemy.Clear();
        }

        private readonly struct ActiveTargetEffect
        {
            public readonly LockOnTargetEffectView View;
            public readonly bool IsShootable;

            public ActiveTargetEffect(LockOnTargetEffectView view, bool isShootable)
            {
                View = view;
                IsShootable = isShootable;
            }
        }
    }
}
