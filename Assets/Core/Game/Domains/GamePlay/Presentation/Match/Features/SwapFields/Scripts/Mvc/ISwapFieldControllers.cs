using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.SwapFields.Scripts.Mvc
{
    public interface ISwapFieldControllers
    {
        void InitEntryPoint();
        void CreateSwapField(ushort swapFieldId, float swapFieldRadius, Vector2 position);
        void SetSwapFieldTransform(ushort swapFieldId, Vector2 position, float radius);
        void DestroySwapField(ushort swapFieldId);
        void DestroyAll();
    }
}