using System.Threading;
using Core.Scripts.Extensions;
using Core.Scripts.Helpers;
using Core.Scripts.Utils;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Features.Player.Scripts.Mvc
{
    public class PlayerEyesView : MonoBehaviour
    {
        [SerializeField] private Transform _leftEyeBall;
        [SerializeField] private Transform _leftEye;
        [SerializeField] private Transform _rightEyeBall;
        [SerializeField] private Transform _rightEye;
        [SerializeField] private float _eyeMovementRadius = 0.1f;
        [SerializeField] private Canvas _spinnedEyesCanvas;
        [SerializeField] private UIImageAnimator _spinnedEyesAnimator;
        
        private SpriteRenderer _leftEyeRenderer;
        private SpriteRenderer _rightEyeRenderer;
        private Sprite _defaultLeftEyeSprite;
        private Sprite _defaultRightEyeSprite;

        public void OnCreated()
        {
            _leftEyeRenderer = _leftEye.GetComponent<SpriteRenderer>();
            _rightEyeRenderer = _rightEye.GetComponent<SpriteRenderer>();
            _defaultLeftEyeSprite = _leftEyeRenderer.sprite;
            _defaultRightEyeSprite = _rightEyeRenderer.sprite;
        }

        public void SetIsSpinned(bool isSpinned, CancellationTokenSource cancellationTokenSource)
        {
            _leftEyeRenderer.sprite = isSpinned ? null : _defaultLeftEyeSprite;
            _rightEyeRenderer.sprite = isSpinned ? null : _defaultRightEyeSprite;

            if (isSpinned)
            {
                _spinnedEyesCanvas.enabled = true;
                _spinnedEyesAnimator.PlayAnimation(cancellationTokenSource).Forget();   
            }
            else
            {
                DisableSpinned();
            }
        }
        
        private void DisableSpinned()
        {
            _spinnedEyesAnimator.StopAnimation();
            _spinnedEyesCanvas.enabled = false;
        }

        public void OnDespawned()
        {
            DisableSpinned();
        }

        public void UpdateEyesToLookAtDirection(System.Numerics.Vector2 direction)
        {
            var eyeOffset = new Vector2(direction.X, direction.Y).normalized * _eyeMovementRadius;
            var leftPosition = _leftEye.position.ToVector2XY() + eyeOffset;
            var rightPosition = _rightEye.position.ToVector2XY() + eyeOffset;
            _leftEyeBall.position = new Vector3(leftPosition.x, leftPosition.y, _leftEyeBall.position.z);
            _rightEyeBall.position = new Vector3(rightPosition.x, rightPosition.y, _rightEyeBall.position.z);
        }
    }
}
