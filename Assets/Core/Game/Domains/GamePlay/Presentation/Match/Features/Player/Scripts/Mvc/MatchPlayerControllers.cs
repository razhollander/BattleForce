using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Presentation.Features.Player.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Features.Player.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.DataService;
using Core.Game.Domains.GamePlay.Presentation.Scripts.ScriptableObjects;
using CoreDomain.Scripts.Services.Logger.Base;
using UnityEngine;
using Zenject;
using Vector2 = System.Numerics.Vector2;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.Player.Scripts.Mvc
{
    public class MatchPlayerControllers : IMatchPlayerControllers
    {
        private readonly IMatchDataService _matchDataService;
        private readonly PlayerViewPool _playerPool;
        private readonly PresentationGamePlayConfig _gamePlayConfig;
        private readonly List<MatchPlayerController> _playerControllers = new ();
        private Transform _playersParent;

        public MatchPlayerControllers(IMatchDataService matchDataService, PlayerView playerViewPrefab, DiContainer diContainer, PresentationGamePlayConfig gamePlayConfig)
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
            var playerController = new MatchPlayerController(_playerPool, playerId, _matchDataService, _gamePlayConfig, _playersParent.transform);
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

        private MatchPlayerController GetPlayer(ushort playerId)
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

        public void HidePlayerHealthBar(ushort playerId)
        {
            LogService.LogError("Set health bar off!");
            GetPlayer(playerId).SetIsHealthBarShown(false);
        }

        public void ShowGemGain(ushort playerId, int amount)
        {
            GetPlayer(playerId)?.ShowGemGain(amount);
        }

        public void DestroyAll()
        {
            foreach (var controller in _playerControllers)
            {
                controller.Destroy();
            }
            _playerControllers.Clear();
        }
    }
}