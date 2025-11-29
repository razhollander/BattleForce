using Core.Game.Domains.GamePlay.Presentation.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Shared;
using Core.Scripts.Extensions;
using CoreDomain.Scripts.Extensions;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Features.Player.Scripts
{
    public class PlayerController
    {
        private readonly IMatchDataService _matchDataService;
        public readonly int PlayerId;
        private PlayerView _playerView;

        public PlayerController(int playerId, IMatchDataService matchDataService)
        {
            _matchDataService = matchDataService;
            PlayerId = playerId;
        }

        public void CreatePlayerView(PlayerView playerViewPrefab, Transform parent)
        {
            var playerModel = _matchDataService.GetPlayer(PlayerId);
            _playerView = Object.Instantiate(playerViewPrefab, parent);
            _playerView.name = "Player_" + PlayerId;
            var playerTransform = playerModel.Spaceship.Transform;
            _playerView.SetPositionAndRotation(playerTransform.Position.ToUnity(),
                playerTransform.RotationVector.ToUnityVector2().ToQuaternion());
        }
    }
}