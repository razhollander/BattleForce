using Core.Scripts.Extensions;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.WaterGunStream.Scripts.Mvc
{
    public class WaterGunStreamView : MonoBehaviour
    {
        [SerializeField] private MeshFilter _meshFilter;
        [SerializeField] private MeshRenderer _meshRenderer;
        [SerializeField] private Transform _pivotTransform;

        private static readonly int BendAmountProperty = Shader.PropertyToID("_BendAmount");

        public void Show()
        {
            gameObject.TrySetActive(true);
        }

        public void Hide()
        {
            gameObject.TrySetActive(false);
        }

        public void UpdateStream(System.Numerics.Vector2 aimDirection, float angularVelocity)
        {
            var angle = Mathf.Atan2(aimDirection.Y, aimDirection.X) * Mathf.Rad2Deg;
            _pivotTransform.rotation = Quaternion.Euler(0f, 0f, angle);
            _meshRenderer.material.SetFloat(BendAmountProperty, angularVelocity);
        }
    }
}
