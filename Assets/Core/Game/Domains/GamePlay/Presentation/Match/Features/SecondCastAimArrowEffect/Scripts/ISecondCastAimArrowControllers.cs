using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.SecondCastAimArrowEffect.Scripts
{
    public interface ISecondCastAimArrowControllers
    {
        void InitEntryPoint();
        void InitExitPoint();
        void AddArrow(ushort id, Vector2 position, Vector2 direction);
        void SetArrow(ushort id, Vector2 position, Vector2 direction);
        void TryRemoveArrow(ushort id);
        void DestroyAll();
    }
}
