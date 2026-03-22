using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Models;
using UnityEngine;
using Core.Game.Domains.GamePlay.Presentation.Scripts.Models;
using Core.Game.Domains.GamePlay.Shared.Extensions;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.KOProjectiles.Scripts.Mvc
{
    public class KOProjectileController
    {
        private readonly KOProjectileView _view;
        private readonly MatchKOProjectileModel _model;
        private readonly MatchPlayerModel _casterModel;

        public KOProjectileView View => _view;

        public KOProjectileController(KOProjectileView view, MatchKOProjectileModel model, MatchPlayerModel casterModel)
        {
            _view = view;
            _model = model;
            _casterModel = casterModel;
            _view.Transform.localScale = new Vector3(model.Size, model.Size, 1);
        }

        public void UpdateTransform()
        {
            _view.Transform.localPosition = new Vector3(_model.Position.x, _model.Position.y, 0);

            var projAngle = Mathf.Atan2(_model.Rotation.y, _model.Rotation.x) * Mathf.Rad2Deg;
            _view.Transform.localRotation = Quaternion.Euler(0, 0, projAngle);

            // coil calculation
            var casterPos = new Vector3(_casterModel.Spaceship.Transform.Position.x, _casterModel.Spaceship.Transform.Position.y, 0);
            var distance = Vector3.Distance(casterPos, _view.Transform.localPosition);

            _view.CoilTransform.position = (_view.Transform.localPosition + casterPos) / 2f;

            var dir = _view.Transform.localPosition - casterPos;
            var angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            _view.CoilTransform.rotation = Quaternion.Euler(0, 0, angle);

            _view.CoilSpriteRenderer.size = new Vector2(distance, _view.CoilSpriteRenderer.size.y);
        }
    }
}
