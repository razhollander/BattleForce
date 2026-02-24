using System.Threading;
using Core.Game.Domains.GamePlay.Presentation.Scripts.ScriptableObjects;
using Core.Scripts.Extensions;
using Core.Scripts.Utils;
using CoreDomain.Scripts.Utils;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.Environment.Springs.Scripts.Mvc
{
    public class MatchEnvironmentSpringController
    {
        private readonly PresentationGamePlayConfig _gamePlayConfig;
        private EnvironmentSpringView _view;
        private Transform _viewTransform;

        public MatchEnvironmentSpringController(PresentationGamePlayConfig gamePlayConfig)
        {
            _gamePlayConfig = gamePlayConfig;
        }

        public void CreateView(EnvironmentSpringView viewPrefab, Transform parent, Vector2 position, float rotationDegrees)
        {
            _view = Object.Instantiate(viewPrefab, parent);
            _viewTransform = _view.transform;
            SetTransform(position, rotationDegrees.AngleToQuaternion());
        }

        public void InterpulateTransform(Vector2 position, float rotationDegrees)
        {
            var direction = rotationDegrees.ToRadians().AngleToVector();
            var targetRotation = direction.ToQuaternion();
            var deltaTime = Time.deltaTime;
            var decay = _gamePlayConfig.ExponentialDecay;
            
            var interpulatedRotation = MathUtils.ExpDecay(
                _viewTransform.rotation, 
                targetRotation, 
                decay,
                deltaTime
            );
            
            var interpulatedPosition = MathUtils.ExpDecay(_viewTransform.position, position, decay, deltaTime);
            SetTransform(interpulatedPosition, interpulatedRotation);
        }
        
        private void SetTransform(Vector2 position, Quaternion rotation)
        {
            _viewTransform.position = position;
            _viewTransform.rotation = rotation;
        }
        
        public void PlayBounceAnimation(CancellationTokenSource cancellationTokenSource)
        {
            _view.PlayBounceAnimation(cancellationTokenSource).Forget();
        }

        public void Destroy()
        {
            Object.Destroy(_view.gameObject);
        }
    }
}
