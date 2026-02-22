using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Models;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.Environment.RotatingWheels.Scripts.Mvc
{
    public class MatchEnvironmentRotatingWheelController
    {
        private readonly MatchEnvironmentRotatingWheelModel _model;
        private MatchEnvironmentRotatingWheelView _view;

        public MatchEnvironmentRotatingWheelController(MatchEnvironmentRotatingWheelModel model)
        {
            _model = model;
        }

        public void CreateView(MatchEnvironmentRotatingWheelView viewPrefab, Transform parent)
        {
            _view = Object.Instantiate(viewPrefab, parent);
            _view.transform.position = _model.CenterPosition;
            _view.name = $"Wheel_{_model.Id}";
            UpdateView();
        }

        public Transform GetViewTransform()
        {
             return _view != null ? _view.transform : null;
        }

        public void UpdateView()
        {
            if (_view == null) return;
            _view.transform.rotation = Quaternion.Euler(0, 0, _model.CurrentRotation);
        }

        public void Destroy()
        {
            if (_view != null)
            {
                Object.Destroy(_view.gameObject);
                _view = null;
            }
        }
    }
}
