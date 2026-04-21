using System;
using System.Threading;
using Core.Scripts.Extensions;
using Core.Scripts.Utils;
using CoreDomain.Scripts.Helpers.Pools;
using DG.Tweening;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.YearsOfPainEffect.Scripts
{
    public class YearsOfPainView : MonoBehaviour, IPoolable
    {
        [SerializeField] private float _showDurationInSeconds = 1f;

        public Action Despawn { get; set; }

        public async Awaitable PlayAndDespawn(Transform parentTransform, Vector2 direction, CancellationTokenSource cancellationTokenSource)
        {
            transform.SetParent(parentTransform, false);
            transform.localPosition = Vector3.zero;
            transform.rotation = direction.ToQuaternion();

            try
            {
                await Awaitable.WaitForSecondsAsync(_showDurationInSeconds, cancellationTokenSource.Token);
            }
            finally
            {
                transform.SetParent(null, false);
                Despawn();
            }
        }

        public void OnCreated()
        {

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
