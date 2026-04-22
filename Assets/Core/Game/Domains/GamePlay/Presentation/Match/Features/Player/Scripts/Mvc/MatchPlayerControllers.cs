using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Presentation.Features.Player.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Features.Player.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.DataService;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.StageCancellationToken;
using Core.Game.Domains.GamePlay.Presentation.Scripts.ScriptableObjects;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Scripts.Network;
using Sirenix.Utilities;
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
        private readonly NetworkConfig _networkConfig;
        private readonly IStageCancellationTokenProvider _stageCancellationTokenProvider;
        private readonly List<MatchPlayerController> _playerControllers = new ();
        private Transform _playersParent;

        public MatchPlayerControllers(IMatchDataService matchDataService, PlayerView playerViewPrefab, DiContainer diContainer, PresentationGamePlayConfig gamePlayConfig,
            NetworkConfig networkConfig, IStageCancellationTokenProvider stageCancellationTokenProvider)
        {
            _matchDataService = matchDataService;
            _playerPool = new PlayerViewPool(playerViewPrefab, diContainer);
            _gamePlayConfig = gamePlayConfig;
            _networkConfig = networkConfig;
            _stageCancellationTokenProvider = stageCancellationTokenProvider;
        }

        public void InitEntryPoint()
        {
            _playersParent = (new GameObject("PlayersParent")).transform;
            _playerPool.InitPool();
        }

        public void AddPlayer(ushort playerId)
        {
            var playerController = new MatchPlayerController(_playerPool, playerId, _matchDataService, _gamePlayConfig, _networkConfig, _playersParent.transform, _stageCancellationTokenProvider);
            playerController.CreatePlayerView();
            _playerControllers.Add(playerController);
        }

        public void UpdatePlayersTickDeltas()
        {
            foreach (var playerController in _playerControllers)
            {
                playerController.UpdateTickDeltas();
            }
        }

        public void UpdatePlayersBulletCooldowns()
        {
            foreach (var playerController in _playerControllers)
            {
                playerController.UpdateBulletCooldown();
            }
        }

        public void UpdatePlayersTalentCooldowns(int currentServerTick)
        {
            foreach (var playerController in _playerControllers)
            {
                playerController.UpdateTalentCooldown(currentServerTick);
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

        public void SetPlayerSentryGunState(ushort playerId, bool isSentryGun)
        {
            GetPlayer(playerId).SetSentryGunState(isSentryGun, _stageCancellationTokenProvider.CancellationTokenSource);
        }

        public void SetPlayersSpinnedState(ushort playerId, bool isOn)
        {
            GetPlayer(playerId).SetPlayersSpinnedState(isOn);
        }

        public void SetPlayerUmbrellaState(ushort playerId, bool isUmbrella)
        {
            GetPlayer(playerId).SetUmbrellaState(isUmbrella);
        }

        public void SetPlayerChickenState(ushort playerId, bool isChicken)
        {
            GetPlayer(playerId).SetChickenState(isChicken);
        }

        public void PlayLayEggAnimation(ushort playerId)
        {
            GetPlayer(playerId).PlayLayEggAnimation(_stageCancellationTokenProvider.CancellationTokenSource);
        }

        public void PlayerYearsOfPainForPlayer(ushort playerId, Vector2 direction)
        {
            GetPlayer(playerId).PlayerYearsOfPain(direction);
        }

        public void SetIsDeadAuraEnabled(ushort playerId, bool isEnabled)
        {
            GetPlayer(playerId).SetIsDeadAuraEnabled(isEnabled);
        }

        public void UpdateIsPlayerArrowShownAccordingToTalentState(ushort playerId, TalentStateS2C talentStateS2C)
        {
            GetPlayer(playerId).UpdateIsArrowShownAccordingToTalentState(talentStateS2C);
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

        public Transform GetPlayerSpaceshipTransform(ushort playerId)
        {
            return GetPlayer(playerId).GetSpaceShipTransform();
        }
        
        public Transform GetPlayerTransform(ushort playerId)
        {
            return GetPlayer(playerId).GetTransform();
        }

        public void HidePlayerHealthBar(ushort playerId)
        {
            GetPlayer(playerId).SetIsHealthBarShown(false);
        }

        public void DestroyAll()
        {
            foreach (var controller in _playerControllers)
            {
                controller.Destroy();
            }
            _playerControllers.Clear();
        }

        public void SetPlayerTalentSelected(ushort playerId, int talentIndex)
        {
            GetPlayer(playerId).SetSelectedTalent(talentIndex);
        }

        public void SetIsTailWaving(ushort playerId, bool isWaving)
        {
            GetPlayer(playerId).SetIsTailWaving(isWaving);
        }
    }
}