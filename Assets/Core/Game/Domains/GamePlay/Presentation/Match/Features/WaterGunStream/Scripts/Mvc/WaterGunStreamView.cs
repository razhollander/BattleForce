using Core.Scripts.Extensions;
using Core.Scripts.Utils;
using CoreDomain.Scripts.Utils;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.WaterGunStream.Scripts.Mvc
{
    public class WaterGunStreamView : MonoBehaviour
    {
        [SerializeField] private Transform _pivotTransform;
        
        public void Show()
        {
            gameObject.TrySetActive(true);
        }

        public void Hide()
        {
            gameObject.TrySetActive(false);
        }

        public void UpdateStreamRotation(System.Numerics.Vector2 aimDirection, float decay)
        {
            var targetRotation = aimDirection.ToQuaternion();
            _pivotTransform.rotation = MathUtils.ExpDecay(_pivotTransform.rotation, targetRotation, decay, Time.deltaTime);
        }
    }
}
