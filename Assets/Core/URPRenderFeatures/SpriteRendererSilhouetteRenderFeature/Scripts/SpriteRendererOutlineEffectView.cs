using UnityEngine;

namespace Core.URPRenderFeatures.SpriteRendererSilhouetteRenderFeature.Scripts
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class SpriteRendererOutlineEffectView : MonoBehaviour
    {
        private SpriteRenderer _renderer;

        private void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
        }

        private void OnEnable()
        {
            SpriteRendererOutlinePass.RegisterRenderer(_renderer);
        }

        private void OnDisable()
        {
            UnregisterRenderer();
        }

        private void OnDestroy()
        {
            UnregisterRenderer();
        }

        private void UnregisterRenderer()
        {
            SpriteRendererOutlinePass.UnregisterRenderer(_renderer);
        }
    }
}