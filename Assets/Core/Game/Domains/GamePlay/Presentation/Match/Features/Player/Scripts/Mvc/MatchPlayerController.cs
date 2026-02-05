using Core.Game.Domains.GamePlay.Presentation.Features.Player.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Features.Player.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.DataService;
using Core.Game.Domains.GamePlay.Presentation.Scripts.ScriptableObjects;
using Core.Scripts.Extensions;
using UnityEngine;
using Vector2 = System.Numerics.Vector2;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.Player.Scripts.Mvc
{
    public class MatchPlayerController
    {
        private readonly IMatchDataService _matchDataService;
        private readonly PresentationGamePlayConfig _gamePlayConfig;
        private readonly Transform _parent;
        public readonly ushort PlayerId;
        private PlayerView _playerView;
        private readonly PlayerViewPool _playerPool;

        public MatchPlayerController(PlayerViewPool playerPool, ushort playerId, IMatchDataService matchDataService, PresentationGamePlayConfig gamePlayConfig, Transform parent)
        {
            _playerPool = playerPool;
            _matchDataService = matchDataService;
            _gamePlayConfig = gamePlayConfig;
            _parent = parent;
            PlayerId = playerId;
        }

        public void CreatePlayerView()
        {
            var playerModel = _matchDataService.GetPlayer(PlayerId);
            _playerView = _playerPool.Spawn();
            _playerView.transform.SetParent(_parent);
            _playerView.name = "Player_" + PlayerId;
            var playerTransform = playerModel.Spaceship.Transform;
            _playerView.SetColor(_gamePlayConfig.ColorPerTeamId[playerModel.TeamId]);
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
            if (Mathf.Approximately(cooldownSecondsLeft, maxShootCooldown))
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

        public void SetIsHealthBarShown(bool isShown)
        {
            _playerView.SetIsHealthBarShown(isShown);
        }

        public void ShowGemGain(int amount)
        {
            _playerView.ShowGemGain(amount);
        }

        public void Destroy()
        {
            _playerView.Despawn();
        }
    }
}