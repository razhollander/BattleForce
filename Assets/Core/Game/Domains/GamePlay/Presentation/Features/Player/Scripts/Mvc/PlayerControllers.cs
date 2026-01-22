using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Presentation.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Presentation.Scripts.ScriptableObjects;
using UnityEngine;
using Zenject;
using Vector2 = System.Numerics.Vector2;

namespace Core.Game.Domains.GamePlay.Presentation.Features.Player.Scripts.Mvc
{
    public class PlayerControllers : IPlayerControllers
    {
        private readonly IMatchDataService _matchDataService;
        private readonly PlayerPool _playerPool;
        private readonly PresentationGamePlayConfig _gamePlayConfig;
        private readonly List<PlayerController> _playerControllers = new ();
        private Transform _playersParent;

        public PlayerControllers(IMatchDataService matchDataService, PlayerView playerViewPrefab, DiContainer diContainer, PresentationGamePlayConfig gamePlayConfig)
        {
            _matchDataService = matchDataService;
            _playerPool = new PlayerPool(playerViewPrefab, diContainer);
            _gamePlayConfig = gamePlayConfig;
        }

        public void InitEntryPoint()
        {
            _playersParent = (new GameObject("PlayersParent")).transform;
            _playerPool.InitPool();
        }

        public void AddPlayer(ushort playerId)
        {
            var playerController = new PlayerController(_playerPool, playerId, _matchDataService, _gamePlayConfig, _playersParent.transform);
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

        private PlayerController GetPlayer(ushort playerId)
        {
            return _playerControllers.Find(x => x.PlayerId == playerId);
        }

        public void SetPlayerHealth(ushort playerId, ushort currentHealth, ushort maxHealth)
        {
            GetPlayer(playerId).SetHealth(currentHealth, maxHealth);
        }

        public void SetPlayerTransform(ushort playerId, Vector2 position, Vector2 direction)
        {
            GetPlayer(playerId).SetTransform(position, direction);
        }

        public UnityEngine.Vector2 GetPlayerPosition(ushort playerId)
        {
            return GetPlayer(playerId).GetPosition();
        }

        public Transform GetPlayerTranform(ushort playerId)
        {
            return GetPlayer(playerId).GetTransform();
        }
    }
}