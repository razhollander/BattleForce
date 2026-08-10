using CoreDomain.Scripts.Utils;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.ScoreGate.Scripts.Mvc
{
    public class ScoreGateController
    {
        private const string SCORE_GATE_NAME = "ScoreGate_";

        private readonly ushort _scoreGateId;
        private readonly ScoreGatePool _pool;
        private readonly Transform _parent;
        private ScoreGateView _view;

        public ushort ScoreGateId => _scoreGateId;
        public Vector2 Position => _view.Transform.position;

        public ScoreGateController(ushort scoreGateId, ScoreGatePool pool, Transform parent)
        {
            _scoreGateId = scoreGateId;
            _pool = pool;
            _parent = parent;
        }

        public void CreateView(Vector2 position, Quaternion rotation, Vector2 postSize, float gapWidth)
        {
            _view = _pool.Spawn();
            _view.name = SCORE_GATE_NAME + _scoreGateId;
            _view.transform.SetParent(_parent);
            _view.SetLayout(postSize, gapWidth);
            _view.SetTransform(position, rotation);
        }

        public void InterpolateTransform(Vector2 position, Quaternion rotation, float decay)
        {
            var lerpedPosition = MathUtils.ExpDecay(_view.Transform.position, position, decay, Time.deltaTime);
            var lerpedRotation = MathUtils.ExpDecay(_view.Transform.rotation, rotation, decay, Time.deltaTime);
            _view.SetTransform(lerpedPosition, lerpedRotation);
        }

        public void SetTeamColor(Color color)
        {
            _view.SetTeamColor(color);
        }

        public void PlayPassAnimation()
        {
            _view.PlayPassAnimation();
        }

        // The indicator shows what the next pass will award. x1 carries no bonus, so it is left blank and only x2+ shows.
        public void SetScoreMultiplier(byte scoreMultiplier)
        {
            _view.SetMultiplierText(scoreMultiplier > 1 ? $"x{scoreMultiplier}" : string.Empty);
        }

        public void Destroy()
        {
            _view.Despawn();
        }
    }
}
