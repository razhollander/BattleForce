using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Features.LockOnHeartSight
{
    public class LockOnHeartSightView : MonoBehaviour
    {
        public void SetIsShown(bool isShown)
        {
            gameObject.SetActive(isShown);
        }
    }
}
