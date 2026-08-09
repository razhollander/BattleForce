using System;
using CoreDomain.Scripts.Helpers.Pools;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.ScoreGate.Scripts.Mvc
{
    // Dumb view: two square posts with a gap between them. The controller drives its transform, layout, and team tint;
    // the view holds no logic and never talks back.
    public class ScoreGateView : MonoBehaviour, IPoolable
    {
        [SerializeField] private Transform _leftPost;
        [SerializeField] private Transform _rightPost;
        [SerializeField] private SpriteRenderer[] _tintableRenderers;

        public Transform Transform { get; private set; }
        public Action Despawn { get; set; }

        public void SetTransform(Vector2 position, Quaternion rotation)
        {
            Transform.SetPositionAndRotation(position, rotation);
        }

        // Posts sit on the local X axis at +/-(gap/2 + postHalfWidth). Sprites are assumed authored at 1 unit, so the
        // localScale is the post size directly. Tune the prefab if the source sprites are a different size.
        public void SetLayout(Vector2 postSize, float gapWidth)
        {
            var postOffsetX = gapWidth * 0.5f + postSize.x * 0.5f;
            var postScale = new Vector3(postSize.x, postSize.y, 1f);

            if (_leftPost != null)
            {
                _leftPost.localPosition = new Vector3(-postOffsetX, 0f, 0f);
                _leftPost.localScale = postScale;
            }

            if (_rightPost != null)
            {
                _rightPost.localPosition = new Vector3(postOffsetX, 0f, 0f);
                _rightPost.localScale = postScale;
            }
        }

        public void SetTeamColor(Color color)
        {
            if (_tintableRenderers == null)
            {
                return;
            }

            foreach (var tintableRenderer in _tintableRenderers)
            {
                tintableRenderer.color = color;
            }
        }

        public void OnCreated()
        {
            Transform = transform;
        }

        public void OnSpawned()
        {
            gameObject.SetActive(true);
        }

        public void OnDespawned()
        {
            gameObject.SetActive(false);
        }
    }
}
