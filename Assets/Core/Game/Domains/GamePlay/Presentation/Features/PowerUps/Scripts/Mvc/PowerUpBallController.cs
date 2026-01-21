using Core.Game.Domains.GamePlay.Presentation.Scripts.MatchModel;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Features.PowerUps.Scripts.Mvc
{
    public class PowerUpBallController
    {
        private readonly IMatchDataService _matchDataService;
        private PowerUpBallView _powerUpBallView;
        public ushort PowerUpBallId { get; private set; }

        public PowerUpBallController(ushort powerUpBallId, IMatchDataService matchDataService)
        {
            PowerUpBallId = powerUpBallId;
            _matchDataService = matchDataService;
        }

        public void CreateView(PowerUpBallPool pool, Transform parent, Vector2 position)
        {
            _powerUpBallView = pool.Spawn(parent);
            _powerUpBallView.SetPosition(position);
        }

        public void DestroyView(PowerUpBallPool pool)
        {
            pool.Despawn(_powerUpBallView);
        }

        public Vector2 GetPosition()
        {
            return _powerUpBallView.transform.position;
        }

        public void InterpolatePosition(float lerpFactor)
        {
            var powerUpBall = _matchDataService.GetPowerUpBall(PowerUpBallId);
            var position = powerUpBall.Position;
            _powerUpBallView.InterpolatePosition(position, lerpFactor);
        }
    }
}
