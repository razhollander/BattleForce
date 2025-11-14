using System.Threading;
using CoreDomain.Scripts.Extensions;
using DG.Tweening;
using UnityEngine;

namespace CoreDomain.Scripts.Mvc.WorldCamera
{
    public class WorldCameraView : MonoBehaviour
    {
        [Header("Focus On Target Settings")]
        [SerializeField] private float _distanceToTarget = 21.26f;
        [SerializeField] private float _distanceToTargetDamping = 4.0f;
        [SerializeField] private float _heightAboveTarget = 7.7f;
        [SerializeField] private float _heightAboveTargetDamping = 4.0f;
        [SerializeField] private float _viewRotationAngle = 43.4f;
        [SerializeField] private float _viewRotationDamping = 3.0f;
        [SerializeField] private Vector3 _offsetFromTarget = new Vector3(0, 0.85f, -1f);

        [Header("Focus On Target Animation Settings")]
        [SerializeField] private float _animationDurationInSeconds;
        [SerializeField] private Vector3 _startPosition;
        [SerializeField] private Vector3 _startRotationEular;
        [SerializeField] private Ease _animationEase;
        
        private float _currentDistanceToTarget;
        private string _lockOnTargetAnimationClipName;

        public void SetPositionRelativeToTarget(Transform target)
        {
            var wantedRotationAngle = _viewRotationAngle;
            var wantedHeight = target.position.y + _heightAboveTarget;
            var wantedDistance = _distanceToTarget;
            
            _currentDistanceToTarget = wantedDistance;
            var currentRotation = Quaternion.Euler(0, wantedRotationAngle, 0);
            var newPosition = target.position - currentRotation * Vector3.forward * _currentDistanceToTarget;
            newPosition += _offsetFromTarget;
            newPosition.y = wantedHeight;
            transform.position = newPosition;
        }

        public void LerpPositionRelativeToTarget(Transform target)
        {
            var wantedRotationAngle = _viewRotationAngle;
            var wantedHeight = target.position.y + _heightAboveTarget;
            var wantedDistance = _distanceToTarget;
            var currentRotationAngle = transform.eulerAngles.y;
            var currentHeight = transform.position.y;

            currentRotationAngle = Mathf.LerpAngle(currentRotationAngle, wantedRotationAngle, _viewRotationDamping * Time.deltaTime);
            currentHeight = Mathf.Lerp(currentHeight, wantedHeight, _heightAboveTargetDamping * Time.deltaTime);
            _currentDistanceToTarget = Mathf.Lerp(_currentDistanceToTarget, wantedDistance, _distanceToTargetDamping * Time.deltaTime);
            var currentRotation = Quaternion.Euler(0, currentRotationAngle, 0);
            var newPosition = target.position - currentRotation * Vector3.forward * _currentDistanceToTarget;
            newPosition += _offsetFromTarget;
            newPosition.y = currentHeight;
            transform.position = newPosition;
        }
        
        public void LookAtTarget(Transform target)
        {
            transform.LookAt(target.position + _offsetFromTarget);
        }

        public async Awaitable DoLockOnTargetAnimation(Vector3 endPosition, Vector3 endRotationEuler,
            CancellationTokenSource cancellationTokenSource)
        {
            transform.position = _startPosition;
            transform.rotation = Quaternion.Euler(_startRotationEular);
            var seq = DOTween.Sequence();
            seq.Join(transform.DOMove(endPosition, _animationDurationInSeconds).SetEase(_animationEase));
            seq.Join(transform.DORotate(endRotationEuler, _animationDurationInSeconds).SetEase(_animationEase));
            await seq.WithCancellationSafe(cancellationTokenSource.Token);
        }
    }
}
