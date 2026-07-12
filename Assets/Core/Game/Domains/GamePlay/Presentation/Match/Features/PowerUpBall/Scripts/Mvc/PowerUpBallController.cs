using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.DataService;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.PowerUps.Scripts.Mvc
{
    public class PowerUpBallController
    {
        private readonly IMatchDataService _matchDataService;
        private readonly PowerUpBallPool _powerUpBallPool;
        private readonly Transform _parent;
        private PowerUpBallView _powerUpBallView;
        public ushort PowerUpBallId { get; private set; }

        public PowerUpBallController(ushort powerUpBallId, IMatchDataService matchDataService, PowerUpBallPool powerUpBallPool, Transform parent)
        {
            PowerUpBallId = powerUpBallId;
            _matchDataService = matchDataService;
            _powerUpBallPool = powerUpBallPool;
            _parent = parent;
        }

        public void CreateView(Vector2 position)
        {
            _powerUpBallView = _powerUpBallPool.Spawn();
            _powerUpBallView.transform.SetParent(_parent);
            _powerUpBallView.SetPosition(position);
        }

        public void DestroyView()
        {
            _powerUpBallView.Despawn();
        }

        public Vector2 GetPosition()
        {
            return _powerUpBallView.transform.position;
        }

        public Transform GetTransform()
        {
            return _powerUpBallView.transform;
        }

        public void InterpolatePosition(float decay)
        {
            var powerUpBall = _matchDataService.GetPowerUpBall(PowerUpBallId);
            var position = powerUpBall.Position;
            _powerUpBallView.InterpolatePosition(position, decay);
        }
    }
}
