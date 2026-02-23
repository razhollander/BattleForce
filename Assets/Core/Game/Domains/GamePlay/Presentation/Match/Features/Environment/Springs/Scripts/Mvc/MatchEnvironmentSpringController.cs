using System.Threading;
using Core.Scripts.Extensions;
using Core.Scripts.Utils;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.Environment.Springs.Scripts.Mvc
{
    public class MatchEnvironmentSpringController
    {
        private EnvironmentSpringView _view;

        public void CreateView(EnvironmentSpringView viewPrefab, Transform parent, Vector2 position, float rotationDegrees)
        {
            _view = Object.Instantiate(viewPrefab, parent);
            UpdateViewTransform(position, rotationDegrees);
        }

        public void UpdateViewTransform(Vector2 position, float rotationDegrees)
        {
            _view.transform.position = position;
            _view.transform.rotation = rotationDegrees.AngleToQuaternion();
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
