using System;
using System.Threading;
using CoreDomain.Scripts.Helpers.Pools;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Features.ChickenEggs.Scripts.Mvc
{
    public class ChickenEggView : MonoBehaviour, IPoolable
    {
        private static readonly int OUTLINE_SHADER_PROPERTY = Shader.PropertyToID("_OutlineColor");

        [SerializeField] private SpriteRenderer _eggSpriteRenderer;
        [SerializeField] private SpriteRenderer _borkenEggSpriteRenderer;
        [SerializeField] private float _brokenDurationInSeconds;
        
        private Material _eggMaterial;

        private CancellationTokenSource _breakCancellationTokenSource;
        public Action Despawn { get; set; }

        public void Setup(Vector3 position, Color outlineColor)
        {
            transform.position = position;
            _eggMaterial.SetColor(OUTLINE_SHADER_PROPERTY, outlineColor);
        }

        public async Awaitable PlayBreakAnimation(CancellationTokenSource cancellationTokenSource)
        {
            _eggSpriteRenderer.enabled = false;
            _borkenEggSpriteRenderer.enabled = true;
            _breakCancellationTokenSource?.Cancel();
            _breakCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationTokenSource.Token);
            await Awaitable.WaitForSecondsAsync(_brokenDurationInSeconds, _breakCancellationTokenSource.Token);
        }

        public void OnCreated()
        {
            _eggMaterial = _eggSpriteRenderer.material;
        }
        
        public void OnSpawned()
        {
            gameObject.SetActive(true);
            _eggSpriteRenderer.enabled = true;
            _borkenEggSpriteRenderer.enabled = false;
        }

        public void OnDespawned()
        {
            _breakCancellationTokenSource?.Cancel();
            _breakCancellationTokenSource = null;
            gameObject.SetActive(false);
        }
    }
}
