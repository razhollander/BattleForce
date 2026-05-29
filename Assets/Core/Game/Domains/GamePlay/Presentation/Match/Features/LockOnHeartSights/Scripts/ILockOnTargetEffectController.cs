using Core.Scripts.Utils.CustomCollections;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.LockOnHeartSights.Scripts
{
    public interface ILockOnTargetEffectController
    {
        void InitEntryPoint();
        void RefreshTargetEffectsOfCaster(ushort casterPlayerId, FixedUnorderedList<ushort> playerIdsLockedOnTarget);
        void UpdateTargetsPositionOnPlayer(ushort casterPlayerId, ushort targetPlayerId, Vector2 startPoint, Vector2 endPoint);
    }
}
