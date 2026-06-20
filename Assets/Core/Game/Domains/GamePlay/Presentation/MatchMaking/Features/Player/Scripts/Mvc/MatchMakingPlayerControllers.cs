using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Presentation.Features.Player.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Features.Player.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.MatchMaking.Scripts.DataService;
using Core.Game.Domains.GamePlay.Presentation.Scripts.ScriptableObjects;
using UnityEngine;
using Zenject;
using Vector2 = System.Numerics.Vector2;

namespace Core.Game.Domains.GamePlay.Presentation.MatchMaking.Features.Player.Scripts.Mvc
{
    public class MatchMakingPlayerControllers : IMatchMakingPlayerControllers
    {
        private readonly IMatchMakingDataService _matchDataService;
        private readonly PlayerViewPool _playerPool;
        private readonly PresentationGamePlayConfig _gamePlayConfig;
        private readonly List<MatchMakingPlayerController> _playerControllers = new ();
        private Transform _playersParent;

        public MatchMakingPlayerControllers(IMatchMakingDataService matchDataService, PlayerView playerViewPrefab, DiContainer diContainer, PresentationGamePlayConfig gamePlayConfig)
        {
            _matchDataService = matchDataService;
            _playerPool = new PlayerViewPool(playerViewPrefab, diContainer);
            _gamePlayConfig = gamePlayConfig;
        }

        public void InitEntryPoint()
        {
            _playersParent = (new GameObject("PlayersParent")).transform;
            _playerPool.InitPool();
        }

        public void AddPlayer(ushort playerId)
        {
            var playerController = new MatchMakingPlayerController(_playerPool, playerId, _matchDataService, _gamePlayConfig, _playersParent.transform);
            playerController.CreatePlayerView();
            _playerControllers.Add(playerController);
        }

        public void UpdatePlayersTransform()
        {
            foreach (var playerController in _playerControllers)
            {
                playerController.UpdateTransform();
            }
        }

        public void UpdatePlayersBulletCooldowns()
        {
            foreach (var playerController in _playerControllers)
            {
                playerController.UpdateBulletCooldown();
            }
        }

        public void ShootBulletEffectForPlayer(ushort playerId)
        {
            GetPlayer(playerId).DoShootEffect();
        }

        private MatchMakingPlayerController GetPlayer(ushort playerId)
        {
            return _playerControllers.Find(x => x.PlayerId == playerId);
        }

        public void SetPlayerTransform(ushort playerId, Vector2 position, Vector2 direction)
        {
            GetPlayer(playerId).SetTransform(position, direction);
        }

        public UnityEngine.Vector2 GetPlayerPosition(ushort playerId)
        {
            return GetPlayer(playerId).GetPosition();
        }

        public Transform GetPlayerTransform(ushort playerId)
        {
            return GetPlayer(playerId).GetTransform();
        }

        public void UpdatePlayerColor(ushort playerId, Color color)
        {
            var player = GetPlayer(playerId);
            if (player != null)
            {
                player.SetColor(color);
            }
        }

        public void SetIsLockOnHeartSightShownForPlayer(ushort playerId, bool isShown)
        {
            GetPlayer(playerId)?.SetIsLockOnHeartSightShown(isShown);
        }
    }
}
