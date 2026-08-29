using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Shared.Scripts.Utils;
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
        private readonly NetworkConfig _networkConfig;
        private readonly IStateMachineService _stateMachineService;
        private readonly LockOnTargetEffectPool _effectsPool;

        private readonly Dictionary<LockOnTargetKey, LockOnTargetEffectView> _activeEffectsPerTarget;
        private readonly List<LockOnTargetKey> _cachedTargetsToRemove;
        private readonly ushort _casterPlayerId;

        public PlayerLockOnTargetEffectController(ushort casterPlayerId, LockOnTargetEffectPool effectsPool,
            SharedGamePlayConfig sharedGamePlayConfig, NetworkConfig networkConfig, IStateMachineService stateMachineService)
        {
            _casterPlayerId = casterPlayerId;
            _sharedGamePlayConfig = sharedGamePlayConfig;
            _networkConfig = networkConfig;
            _stateMachineService = stateMachineService;
            _effectsPool = effectsPool;
            _activeEffectsPerTarget = new Dictionary<LockOnTargetKey, LockOnTargetEffectView>(networkConfig.MaxCap.ConcurrentPlayers - 1);
            _cachedTargetsToRemove = new List<LockOnTargetKey>(networkConfig.MaxCap.ConcurrentPlayers - 1);
        }

        public void RefreshTargetEffectsOfCaster(FixedUnorderedList<ObjectLockedOnTargetS2C> playerIdsLockedOnTarget)
        {
            DespawnTargetsWhichArentShownAnymore(playerIdsLockedOnTarget);

            foreach (var target in playerIdsLockedOnTarget.AsSpan())
            {
                UpdateLockOnTargetEffectForTarget(target);
            }
        }

        private void UpdateLockOnTargetEffectForTarget(ObjectLockedOnTargetS2C target)
        {
            var targetKey = target.GetKey();
            var isShootable = target.IsLockOnTargetShootable;

            if (!_activeEffectsPerTarget.TryGetValue(targetKey, out var _))
            {
                var newTargetEffectView = _effectsPool.Spawn();
                newTargetEffectView.Setup(_sharedGamePlayConfig.LockOnTargetDurationInSeconds);
                _activeEffectsPerTarget[targetKey] = newTargetEffectView;
            }
            PlayAnimationForState(_activeEffectsPerTarget[targetKey], isShootable);
        }

        private void DespawnTargetsWhichArentShownAnymore(FixedUnorderedList<ObjectLockedOnTargetS2C> playerIdsLockedOnTarget)
        {
            _cachedTargetsToRemove.Clear();
            foreach (var targetKey in _activeEffectsPerTarget.Keys)
            {
                if (!playerIdsLockedOnTarget.ContainsTarget(targetKey))
                {
                    _cachedTargetsToRemove.Add(targetKey);
                }
            }

            foreach (var targetKey in _cachedTargetsToRemove)
            {
                _activeEffectsPerTarget[targetKey].Despawn();
                _activeEffectsPerTarget.Remove(targetKey);
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

        public void UpdateTargetRetentionProgress(ObjectLockedOnTargetS2C target, int currentTick)
        {
            if (!_activeEffectsPerTarget.TryGetValue(target.GetKey(), out var activeEffect))
            {
                return;
            }

            if (!target.IsLockOnTargetRetained)
            {
                activeEffect.HideRetentionEffect();
                return;
            }

            var retentionSecondsLeft = TickUtils.GetSecondsLeftUntilTick(currentTick, target.RetentionEndTick, _networkConfig.DeltaTime);
            var retentionProgress = Mathf.Clamp01(retentionSecondsLeft / _sharedGamePlayConfig.LockOnTargetRetentionDurationInSeconds);
            activeEffect.ShowRetentionEffect(retentionProgress);
        }

        public void UpdateTargetsPositionOnPlayer(LockOnTargetKey targetKey, Vector2 startPoint, Vector2 endPoint)
        {
            if (!_activeEffectsPerTarget.TryGetValue(targetKey, out var activeEffect))
            {
                LogService.LogError($"No effect for caster player id: {_casterPlayerId} on target id: {targetKey.TargetId} type: {targetKey.TargetType}");
                return;
            }

            activeEffect.UpdatePosition(startPoint, endPoint, endPoint);
        }

        public void DestroyAll()
        {
            foreach (var activeEffect in _activeEffectsPerTarget.Values)
            {
                activeEffect.Despawn();
            }

            _activeEffectsPerTarget.Clear();
        }
    }
}
