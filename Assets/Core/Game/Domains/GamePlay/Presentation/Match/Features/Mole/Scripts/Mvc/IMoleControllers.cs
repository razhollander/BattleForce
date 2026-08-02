using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.Mole.Scripts.Mvc
{
    public interface IMoleControllers
    {
        void InitEntryPoint();
        void CreateMole(ushort moleId, Vector2 position);
        Vector2 GetMolePosition(ushort moleId);
        void DestroyMoleWithHitEffect(ushort moleId);
        void DestroyMoleWithExpireEffect(ushort moleId);
        void DestroyAll();
    }
}
