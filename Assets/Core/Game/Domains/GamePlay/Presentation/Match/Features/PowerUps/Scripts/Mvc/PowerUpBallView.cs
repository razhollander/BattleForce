using System;
using CoreDomain.Scripts.Helpers.Pools;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.PowerUps.Scripts.Mvc
{
    public class PowerUpBallView : MonoBehaviour, IPoolable
    {
        public void InterpolatePosition(Vector2 position, float lerpFactor)
        {
            var lerpedPosition = Vector2.Lerp(transform.position, position, lerpFactor);
            SetPosition(lerpedPosition);
        }

        public void SetPosition(Vector2 position)
        {
            transform.position = position;
        }

        public void OnCreated()
        {
        }

        public Action Despawn { get; set; }

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
