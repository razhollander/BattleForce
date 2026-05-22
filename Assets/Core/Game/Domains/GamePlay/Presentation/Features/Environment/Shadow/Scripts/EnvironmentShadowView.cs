using Core.Scripts.Utils.Shadows;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Features.Environment.Shadow.Scripts
{
    [RequireComponent(typeof(Renderer))]
    public class EnvironmentShadowView : MonoBehaviour
    {
        private Renderer _renderer;
        
        private void Awake()
        {
            _renderer = GetComponent<Renderer>();
        }

        private void OnEnable()
        {
            if (_renderer is SpriteRenderer sr)
            {
                SpriteShadowPass.RegisterRenderer(sr);
            }
            //SpriteShadowPass.RegisterRenderer(_renderer);
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
            if (_renderer is SpriteRenderer sr)
            {
                SpriteShadowPass.UnregisterRenderer(sr);
            }
            //SpriteShadowPass.UnregisterRenderer(_renderer);
        }
    }
}
