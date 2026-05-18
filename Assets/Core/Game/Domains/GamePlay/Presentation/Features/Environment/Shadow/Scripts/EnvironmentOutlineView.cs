using Core.Scripts.Utils.Shadows;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Features.Environment.Shadow.Scripts
{
    [RequireComponent(typeof(Renderer))]
    public class EnvironmentOutlineView : MonoBehaviour
    {
        private Renderer _renderer;

        private void Awake()
        {
            _renderer = GetComponent<Renderer>();
        }

        private void OnEnable()
        {
            SpriteOutlinePass.RegisterRenderer(_renderer);
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
            SpriteOutlinePass.UnregisterRenderer(_renderer);
        }
    }
}