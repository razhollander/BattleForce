using System.Threading;
using Core.Scripts.Utils;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Core.Scripts.Extensions;

namespace Core.Game.Domains.GamePlay.Presentation.Features.Player.Scripts.Mvc
{
    public class UmbrellaStickView : MonoBehaviour
    {
        [SerializeField] private SpriteAnimator _animator;

        public void PlayAnimation(CancellationTokenSource cancellationTokenSource)
        {
            gameObject.TrySetActive(true);
            _animator.PlayAnimation(cancellationTokenSource).Forget();
        }

        public void StopAnimation()
        {
            _animator.StopAnimation();
            gameObject.TrySetActive(false);
        }
    }
}