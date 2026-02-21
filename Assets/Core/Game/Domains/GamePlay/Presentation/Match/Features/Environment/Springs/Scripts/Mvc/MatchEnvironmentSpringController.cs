using System.Threading;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.Environment.Springs.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Scripts.Models;
using Core.Scripts.Extensions;
using Core.Scripts.Utils;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.Environment.Springs.Scripts.Mvc
{
    public class MatchEnvironmentSpringController
    {
        private EnvironmentSpringView _view;

        public void CreateView(EnvironmentSpringView viewPrefab, Transform parent, Vector2 position, float rotationAngle)
        {
            _view = Object.Instantiate(viewPrefab, parent);
            _view.transform.position = position;
            _view.transform.rotation = rotationAngle.AngleToQuaternion();
        }

        public void PlayBounceAnimation(CancellationTokenSource cancellationTokenSource)
        {
            _view.PlayBounceAnimation(cancellationTokenSource).Forget();
        }

        public void Destroy()
        {
            if (_view != null)
            {
                Object.Destroy(_view.gameObject);
            }
        }
    }
}
