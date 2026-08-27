using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Scripts.Network;
using Core.Scripts.Utils.CustomCollections;
using CoreDomain.Scripts.Services.Logger.Base;
using CoreDomain.Scripts.Services.StateMachineService;
using UnityEngine;
using Zenject;

namespace Core.Game.Domains.GamePlay.Presentation.Features.LockOnTarget
{
    public class PlayersLockOnTargetEffectControllers : ILockOnTargetEffectController
    {
        private readonly SharedGamePlayConfig _sharedGamePlayConfig;
        private readonly NetworkConfig _networkConfig;
        private readonly IStateMachineService _stateMachineService;
        private readonly LockOnTargetEffectPool _effectsPool;

        private readonly Dictionary<ushort, PlayerLockOnTargetEffectController> _targetEffectControllerPerPlayerId = new Dictionary<ushort, PlayerLockOnTargetEffectController>();

        public PlayersLockOnTargetEffectControllers(
            LockOnTargetEffectView prefab, DiContainer diContainer, SharedGamePlayConfig sharedGamePlayConfig, NetworkConfig networkConfig, IStateMachineService stateMachineService)
        {
            _sharedGamePlayConfig = sharedGamePlayConfig;
            _networkConfig = networkConfig;
            _stateMachineService = stateMachineService;
            _effectsPool = new LockOnTargetEffectPool(prefab, diContainer);
        }

        public void InitEntryPoint()
        {
            _effectsPool.InitPool();
        }

        public void AddPlayer(ushort casterPlayerId, FixedUnorderedList<ObjectLockedOnTargetS2C> casterTargetedEnemyIds)
        {
            _targetEffectControllerPerPlayerId[casterPlayerId] = new PlayerLockOnTargetEffectController(casterPlayerId, _effectsPool, _sharedGamePlayConfig,
                _networkConfig, _stateMachineService);
            RefreshTargetEffectsOfCaster(casterPlayerId, casterTargetedEnemyIds);
        }

        public void RefreshTargetEffectsOfCaster(ushort casterPlayerId, FixedUnorderedList<ObjectLockedOnTargetS2C> playerIdsLockedOnTarget)
        {
            if (!_targetEffectControllerPerPlayerId.TryGetValue(casterPlayerId, out var casterActiveEffects))
            {
                LogService.LogError($"Can't refresh effects, no effects for caster player id: {casterPlayerId}");
                return;
            }
            
            casterActiveEffects.RefreshTargetEffectsOfCaster(playerIdsLockedOnTarget);
        }

        public void UpdateTargetsPositionOnPlayer(ushort casterPlayerId, LockOnTargetKey targetKey, Vector2 startPoint, Vector2 endPoint)
        {
            if (!_targetEffectControllerPerPlayerId.TryGetValue(casterPlayerId, out var casterActiveEffects))
            {
                LogService.LogError($"Can't update targets positions, no effects for caster player id: {casterPlayerId}");
                return;
            }

            casterActiveEffects.UpdateTargetsPositionOnPlayer(targetKey, startPoint, endPoint);
        }

        public void UpdateTargetRetentionProgress(ushort casterPlayerId, ObjectLockedOnTargetS2C target, int currentTick)
        {
            if (!_targetEffectControllerPerPlayerId.TryGetValue(casterPlayerId, out var casterActiveEffects))
            {
                return;
            }

            casterActiveEffects.UpdateTargetRetentionProgress(target, currentTick);
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
