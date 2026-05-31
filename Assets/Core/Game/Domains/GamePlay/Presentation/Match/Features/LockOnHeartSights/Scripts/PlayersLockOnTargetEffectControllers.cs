using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.StageCancellationToken;
using Core.Scripts.Network;
using Core.Scripts.Utils.CustomCollections;
using CoreDomain.Scripts.Services.Logger.Base;
using UnityEngine;
using Zenject;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.LockOnHeartSights.Scripts
{
    public class PlayersLockOnTargetEffectControllers : ILockOnTargetEffectController
    {
        private readonly SharedGamePlayConfig _sharedGamePlayConfig;
        private readonly NetworkConfig _networkConfig;
        private readonly IStageCancellationTokenProvider _stageCancellationTokenProvider;
        private readonly LockOnTargetEffectPool _effectsPool;

        private readonly Dictionary<ushort, PlayerLockOnTargetEffectController> _targetEffectControllerPerPlayerId = new Dictionary<ushort, PlayerLockOnTargetEffectController>();

        public PlayersLockOnTargetEffectControllers(
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
            _targetEffectControllerPerPlayerId[casterPlayerId] = new PlayerLockOnTargetEffectController(casterPlayerId, casterTargetedEnemyIds, _effectsPool, _sharedGamePlayConfig,
                _networkConfig, _stageCancellationTokenProvider);
            RefreshTargetEffectsOfCaster(casterPlayerId, casterTargetedEnemyIds);
        }

        public void RefreshTargetEffectsOfCaster(ushort casterPlayerId, FixedUnorderedList<ushort> playerIdsLockedOnTarget)
        {
            if (!_targetEffectControllerPerPlayerId.TryGetValue(casterPlayerId, out var casterActiveEffects))
            {
                LogService.LogError($"Can't refresh effects, no effects for caster player id: {casterPlayerId}");
                return;
            }
            
            casterActiveEffects.RefreshTargetEffectsOfCaster(playerIdsLockedOnTarget);
        }

        public void UpdateTargetsPositionOnPlayer(ushort casterPlayerId, ushort targetPlayerId, Vector2 startPoint, Vector2 endPoint)
        {
            if (!_targetEffectControllerPerPlayerId.TryGetValue(casterPlayerId, out var casterActiveEffects))
            {
                LogService.LogError($"Can't update targets positions, no effects for caster player id: {casterPlayerId}");
                return;
            }

            casterActiveEffects.UpdateTargetsPositionOnPlayer(targetPlayerId, startPoint, endPoint);
        }

        public void DestroyAll()
        {
            foreach (var targetEffectController in _targetEffectControllerPerPlayerId.Values)
            {
                targetEffectController.DestroyAll();
            }
            
            _targetEffectControllerPerPlayerId.Clear();
        }
    }
}
