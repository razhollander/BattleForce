using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Features.LockOnTargetSight
{
    public class LockOnTargetSightView : MonoBehaviour
    {
        public void SetIsShown(bool isShown)
        {
            gameObject.SetActive(isShown);
        }
    }
}
