using System;
using Core.Scripts.Utils.Shadows;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Features.Environment.Shadow.Scripts
{
    [RequireComponent(typeof(Renderer))]
    public class EnvironmentBackgroundView : MonoBehaviour
    {
        private Renderer _renderer;

        private void Awake()
        {
            _renderer = GetComponent<Renderer>();
        }

        private void OnEnable()
        {
            // if (_renderer is SpriteRenderer sr)
            // {
            //     SpriteShadowCommandBufferPass.RegisterRenderer(sr);
            // }
           // SpriteBackgroundPass.RegisterRenderer(_renderer);
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
            // if (_renderer is SpriteRenderer sr)
            // {
            //     SpriteShadowCommandBufferPass.UnregisterRenderer(sr);
            // }
            
            //SpriteBackgroundPass.UnregisterRenderer(_renderer);
        }
    }
}