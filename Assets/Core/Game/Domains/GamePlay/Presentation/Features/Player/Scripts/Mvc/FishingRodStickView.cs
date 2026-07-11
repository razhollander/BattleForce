using Core.Scripts.Extensions;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Features.Player.Scripts.Mvc
{
    public class FishingRodStickView : MonoBehaviour
    {
        [SerializeField] private GameObject _stickParent;
        [SerializeField] private Animation _animation;
        [SerializeField] private AnimationClip _lookLeftAnimationClip;
        [SerializeField] private AnimationClip _lookRightAnimationClip;
        [field: SerializeField] public Transform FishingRodTipPivot { get; private set; }

        private bool? _isLookingRight;

        public void Show()
        {
            _stickParent.TrySetActive(true);
        }

        public void Hide()
        {
            _isLookingRight = null;
            _stickParent.TrySetActive(false);
        }

        public void SetPosition(Vector2 position)
        {
            transform.position = new Vector3(position.x, position.y, transform.position.z);
        }

        public void SetDirection(bool isDirectionRight)
        {
            // Guard against replaying the clip every frame, which would freeze it on its first frame.
            if (_isLookingRight == isDirectionRight)
            {
                return;
            }

            _isLookingRight = isDirectionRight;
            _animation.Play(isDirectionRight ? _lookRightAnimationClip.name : _lookLeftAnimationClip.name);
        }
    }
}
