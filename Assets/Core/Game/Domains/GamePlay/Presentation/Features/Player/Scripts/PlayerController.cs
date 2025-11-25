using Core.Game.Domains.GamePlay.Presentation.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Shared;
using Core.Scripts.Extensions;
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
            _playerView.SetPositionAndRotation(playerModel.TransformState.CurrentPosition.ToUnity(), playerModel.TransformState.CurrentRotationAngle.AngleToQuaternion());
        }
    }
}