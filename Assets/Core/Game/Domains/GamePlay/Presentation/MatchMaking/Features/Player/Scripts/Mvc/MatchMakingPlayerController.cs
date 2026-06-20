using Core.Game.Domains.GamePlay.Presentation.Features.Player.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Features.Player.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.MatchMaking.Scripts.DataService;
using Core.Game.Domains.GamePlay.Presentation.Scripts.ScriptableObjects;
using Core.Scripts.Extensions;
using UnityEngine;
using Vector2 = System.Numerics.Vector2;

namespace Core.Game.Domains.GamePlay.Presentation.MatchMaking.Features.Player.Scripts.Mvc
{
    public class MatchMakingPlayerController
    {
        private readonly IMatchMakingDataService _matchDataService;
        private readonly PresentationGamePlayConfig _gamePlayConfig;
        private readonly Transform _parent;
        public readonly ushort PlayerId;
        private PlayerView _playerView;
        private readonly PlayerViewPool _playerPool;

        public MatchMakingPlayerController(PlayerViewPool playerPool, ushort playerId, IMatchMakingDataService matchDataService, PresentationGamePlayConfig gamePlayConfig, Transform parent)
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
            _playerView.SetPlayerName(playerModel.PlayerName);
            _playerView.SetIsTailWaving(true);
            var playerTransform = playerModel.Spaceship.Transform;
            _playerView.SetColor(_gamePlayConfig.ColorPerTeamId[playerModel.TeamId]);
            _playerView.SetPositionAndRotation(playerTransform.Position.ToUnityVector2(),
                playerTransform.Direction.ToUnityVector2().ToQuaternion());
        }

        public void UpdateTransform()
        {
            var playerModel = _matchDataService.GetPlayer(PlayerId);
            var playerTransformState = playerModel.Spaceship.Transform;
            var playerPosition = playerTransformState.Position.ToUnityVector2();
            var playerRotation = playerTransformState.Direction.ToUnityVector2().ToQuaternion();
            var exponentialDecay = _gamePlayConfig.ExponentialDecay;
            _playerView.InterpolateTransform(playerPosition, playerRotation, exponentialDecay);
            _playerView.UpdateTailBend();
        }

        public void UpdateBulletCooldown()
        {
            var playerModel = _matchDataService.GetPlayer(PlayerId);
            var playerShootState = playerModel.Spaceship.Shoot;
            var maxShootCooldown = playerShootState.MaxCooldown;
            var cooldownSecondsLeft = playerShootState.CooldownSecondsLeft;
            
            _playerView.SetBulletLoading(cooldownSecondsLeft, maxShootCooldown);
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
            return _playerView.GetSpaceShipTransform();
        }

        public void SetColor(Color color)
        {
            _playerView.SetColor(color);
        }

        public void SetIsLockOnHeartSightShown(bool isShown)
        {
            _playerView.SetIsLockOnHeartSightShown(isShown);
        }
    }
}