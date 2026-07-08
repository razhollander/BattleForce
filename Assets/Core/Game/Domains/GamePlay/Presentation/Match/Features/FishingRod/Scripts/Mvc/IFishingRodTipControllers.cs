using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.FishingRod.Scripts.Mvc
{
    public interface IFishingRodTipControllers
    {
        void InitEntryPoint();
        void CreateFishingRodTip(ushort tipId, ushort casterPlayerId, Vector2 position, Vector2 rotation, Vector2 casterPosition);
        void InterpolateFishingRodTipTransform(ushort tipId, Vector2 position, Quaternion rotation, Vector2 casterPosition);
        void DestroyFishingRodTip(ushort tipId);
        void DestroyAll();
    }
}
