using System.Threading;
using Core.Scripts.Utils;
using UnityEngine;
using Core.Scripts.Extensions;
using Core.Scripts.Helpers;

namespace Core.Game.Domains.GamePlay.Presentation.Features.Player.Scripts.Mvc
{
    public class UmbrellaStickView : MonoBehaviour
    {
        //[SerializeField] private SpriteAnimator _animator;
        [SerializeField] private Transform _umbrellaParent;
        public void PlayAnimation(CancellationTokenSource cancellationTokenSource)
        {
            _umbrellaParent.gameObject.TrySetActive(true);
            //_animator.PlayAnimation(cancellationTokenSource).Forget();
        }

        public void StopAnimation()
        {
            //_animator.StopAnimation();
            _umbrellaParent.gameObject.TrySetActive(false);
        }

        public void SetRotation(Quaternion rotation)
        {
            _umbrellaParent.rotation = rotation;
        }
    }
}