using Core.Game.Domains.GamePlay.Presentation.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Presentation.Scripts.ScriptableObjects;
using Core.Game.Domains.GamePlay.Shared;
using Core.Scripts.Extensions;
using Core.Scripts.Network;
using CoreDomain.Scripts.Extensions;
using UnityEngine;
using Vector2 = System.Numerics.Vector2;

namespace Core.Game.Domains.GamePlay.Presentation.Features.Player.Scripts
{
    public class PlayerController
    {
        private readonly IMatchDataService _matchDataService;
        private readonly PresentationGamePlayConfig _gamePlayConfig;
        public readonly int PlayerId;
        private PlayerView _playerView;
        
        public PlayerController(int playerId, IMatchDataService matchDataService, PresentationGamePlayConfig gamePlayConfig)
        {
            _matchDataService = matchDataService;
            _gamePlayConfig = gamePlayConfig;
            PlayerId = playerId;
        }

        public void CreatePlayerView(PlayerView playerViewPrefab, Transform parent)
        {
            var playerModel = _matchDataService.GetPlayer(PlayerId);
            _playerView = Object.Instantiate(playerViewPrefab, parent);
            _playerView.name = "Player_" + PlayerId;
            var playerTransform = playerModel.Spaceship.Transform;
            _playerView.SetPositionAndRotation(playerTransform.Position.ToUnity(),
                playerTransform.Direction.ToUnityVector2().ToQuaternion());
        }

        public void UpdateTransform()
        {
            var playerModel = _matchDataService.GetPlayer(PlayerId);
            var playerTransformState = playerModel.Spaceship.Transform;
            var playerPosition = playerTransformState.Position.ToUnity();
            var playerRotation = playerTransformState.Direction.ToUnityVector2().ToQuaternion();
            var interpolationFactor = _gamePlayConfig.InterpolationFactor;
            _playerView.InterpolateTransform(playerPosition, playerRotation, interpolationFactor);
        }

        public void UpdateBulletCooldown()
        {
            var playerModel = _matchDataService.GetPlayer(PlayerId);
            var playerShootState = playerModel.Spaceship.Shoot;
            var maxShootCooldown = playerShootState.MaxCooldown;
            var cooldownSecondsLeft = playerShootState.CooldownSecondsLeft;
            var interpolationFactor = _gamePlayConfig.InterpolationFactor;
            _playerView.InterpolateBulletLoading(cooldownSecondsLeft, maxShootCooldown, interpolationFactor);
        }

        public void RestoreBulletEffect()
        {
            _playerView.ShowIsBulletAvailable(true);
        }
        
        public void DoShootEffect()
        {
            _playerView.ShowIsBulletAvailable(false);
        }
    }
}