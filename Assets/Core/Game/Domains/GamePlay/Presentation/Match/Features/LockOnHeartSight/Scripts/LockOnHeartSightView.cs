using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.LockOnHeartSights.Scripts
{
    public class LockOnHeartSightView : MonoBehaviour
    {
        public void SetIsShown(bool isShown)
        {
            gameObject.SetActive(isShown);
        }
    }
}
