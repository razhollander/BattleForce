using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.PowerUps.Scripts.Mvc
{
    public interface IPowerUpBallControllers
    {
        void InitEntryPoint();
        void CreatePowerUpBall(ushort powerUpBallId, Vector2 position);
        Vector2 GetPowerUpBallPosition(ushort powerUpBallId);
        Transform GetPowerUpBallTransform(ushort powerUpBallId);
        void DestroyPowerUpBall(ushort cardId);
        void DestroyAll();
        void UpdatePowerUpBallsTransform();
    }
}