using UnityEngine;

namespace Core.URPRenderFeatures.SpriteRendererSilhouetteRenderFeature.Scripts
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class SpriteRendererShadowEffectView : MonoBehaviour
    {
        private SpriteRenderer _renderer;
        
        private void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
        }

        private void OnEnable()
        {
            SpriteRendererShadowPass.RegisterRenderer(_renderer);
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
            SpriteRendererShadowPass.UnregisterRenderer(_renderer);
        }
    }
}
