using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Features.PowerUps.Scripts.Mvc
{
    public class PowerUpBallView : MonoBehaviour
    {
        public void InterpolatePosition(Vector2 position, float lerpFactor)
        {
            var lerpedPosition = Vector2.Lerp(transform.position, position, lerpFactor);
            SetPosition(lerpedPosition);
        }

        public void SetPosition(Vector2 position)
        {
            transform.position = position;
        }
    }
}
