using System.Threading;
using Core.Scripts.Extensions;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Features.Player.Scripts.Mvc
{
    public class YearsOfPainView : MonoBehaviour
    {
        private const string ACTIVATE_ANIMATION = "ActivateYearsOfPain";
        
        [SerializeField] private Animation _animation;
        [SerializeField] private GameObject _lookRightGameObject;
        [SerializeField] private GameObject _lookLeftGameObject;
        
        public async Awaitable PlayAndHide(Vector2 direction, CancellationTokenSource cancellationTokenSource)
        {
            gameObject.SetActive(true);
            transform.rotation = direction.ToQuaternion();
            
            var isLookingRight = Vector2.Dot(Vector2.right, direction) > 0;
            _lookRightGameObject.SetActive(isLookingRight);
            _lookLeftGameObject.SetActive(!isLookingRight);

            try
            {
                await _animation.PlayAsync(ACTIVATE_ANIMATION, cancellationTokenSource.Token);
            }
            finally
            {
                gameObject.SetActive(false);
            }
        }
    }
}
