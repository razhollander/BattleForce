using Core.Game.Domains.GamePlay.Presentation.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Presentation.Scripts.ScriptableObjects;
using Core.Game.Domains.GamePlay.Shared;
using Core.Scripts.Extensions;
using Core.Scripts.Network;
using CoreDomain.Scripts.Services.Logger.Base;
using UnityEngine;
using Vector2 = System.Numerics.Vector2;

namespace Core.Game.Domains.GamePlay.Presentation.Features.Player.Scripts
{
    public class PlayerController
    {
        private readonly IMatchDataService _matchDataService;
        private readonly PresentationGamePlayConfig _gamePlayConfig;
        public readonly ushort PlayerId;
        private PlayerView _playerView;
        
        public PlayerController(ushort playerId, IMatchDataService matchDataService, PresentationGamePlayConfig gamePlayConfig)
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
            _playerView.SetColor(playerModel.Spaceship.Color);
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
            if (cooldownSecondsLeft == maxShootCooldown)
            {
                RestoreBulletEffect();
            }
        }

        public void RestoreBulletEffect()
        {
            _playerView.ShowIsBulletAvailable(true);
        }
        
        public void DoShootEffect()
        {
            _playerView.ShowIsBulletAvailable(false);
        }

        public void SetHealth(ushort currentHealth, ushort maxHealth)
        {
            _playerView.UpdateHealthBar(currentHealth, maxHealth);
        }

        public void SetTransform(Vector2 position, Vector2 direction)
        {
            _playerView.SetPositionAndRotation(position.ToUnityVector2(), direction.ToUnityVector2().ToQuaternion());
        }

        public UnityEngine.Vector2 GetPosition()
        {
            return _playerView.GetPosition();
        }

        public Transform GetTransform()
        {
            return _playerView.GetTransform();
        }
    }
}