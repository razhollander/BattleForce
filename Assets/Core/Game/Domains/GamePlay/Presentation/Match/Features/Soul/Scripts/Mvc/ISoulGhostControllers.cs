using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.Soul.Scripts.Mvc
{
    public interface ISoulGhostControllers
    {
        void InitEntryPoint();
        void CreateSoulGhost(ushort ghostId, ushort casterPlayerId, ushort teamId, Vector2 position, Vector2 rotation);
        void InterpolateSoulGhostTransform(ushort ghostId, Vector2 position, Quaternion rotation);
        void DestroySoulGhost(ushort ghostId);
        void DestroyAll();
    }
}
