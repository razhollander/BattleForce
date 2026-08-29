using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Scripts.Utils.CustomCollections;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Features.LockOnTarget
{
    public interface ILockOnTargetEffectController
    {
        void InitEntryPoint();
        void RefreshTargetEffectsOfCaster(ushort casterPlayerId, FixedUnorderedList<ObjectLockedOnTargetS2C> playerIdsLockedOnTarget);
        void UpdateTargetsPositionOnPlayer(ushort casterPlayerId, LockOnTargetKey targetKey, Vector2 startPoint, Vector2 endPoint);
        void UpdateTargetRetentionProgressOfPlayer(ushort casterPlayerId, ObjectLockedOnTargetS2C target, int currentTick);
        void DestroyAll();
        void AddPlayer(ushort casterPlayerId, FixedUnorderedList<ObjectLockedOnTargetS2C> casterTargetedEnemyIds);
    }
}
