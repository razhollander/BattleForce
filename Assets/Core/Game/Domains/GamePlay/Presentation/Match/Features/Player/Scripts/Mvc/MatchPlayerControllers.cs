using System.Collections.Generic;
using System.Threading;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.DataService;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.StageCancellationToken;
using Core.Game.Domains.GamePlay.Presentation.Scripts.InputBeingUsed;
using Core.Game.Domains.GamePlay.Presentation.Scripts.ScriptableObjects;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Scripts.Network;
using Core.Scripts.Services.AudioService;
using Core.Scripts.Utils;
using UnityEngine;
using Zenject;
using Vector2 = System.Numerics.Vector2;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.Player.Scripts.Mvc
{
    public class MatchPlayerControllers : IMatchPlayerControllers
    {
        private readonly IMatchDataService _matchDataService;
        private readonly MatchPlayerViewPool _playerPool;
        private readonly PresentationGamePlayConfig _gamePlayConfig;
        private readonly SharedGamePlayConfig _sharedGamePlayConfig;
        private readonly NetworkConfig _networkConfig;
        private readonly IStageCancellationTokenProvider _stageCancellationTokenProvider;
        private readonly List<MatchPlayerController> _playerControllers = new ();
        private Transform _playersParent;
        private readonly IAudioService _audioService;

        public MatchPlayerControllers(IMatchDataService matchDataService, MatchPlayerView playerViewPrefab, DiContainer diContainer, PresentationGamePlayConfig gamePlayConfig,
            SharedGamePlayConfig sharedGamePlayConfig, NetworkConfig networkConfig, IStageCancellationTokenProvider stageCancellationTokenProvider, IAudioService audioService)
        {
            _matchDataService = matchDataService;
            _playerPool = new MatchPlayerViewPool(playerViewPrefab, diContainer);
            _gamePlayConfig = gamePlayConfig;
            _sharedGamePlayConfig = sharedGamePlayConfig;
            _networkConfig = networkConfig;
            _stageCancellationTokenProvider = stageCancellationTokenProvider;
            _audioService = audioService;
        }

        public void InitEntryPoint()
        {
            _playersParent = (new GameObject("PlayersParent")).transform;
            _playerPool.InitPool();
        }

        public void AddPlayer(ushort playerId)
        {
            var playerController = new MatchPlayerController(_playerPool, playerId, _matchDataService, _gamePlayConfig, _sharedGamePlayConfig, _networkConfig, _playersParent.transform,
                _stageCancellationTokenProvider, _audioService);
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

        public void SetPlayerCurrentPowerUp(ushort playerId, PowerUpType powerUpType)
        {
            GetPlayer(playerId).SetCurrentPowerUp(powerUpType);
        }

        public void SetPlayerWaterGunState(ushort playerId, bool isOn)
        {
            GetPlayer(playerId).SetWaterGunState(isOn);
        }

        public void SetPlayerFishingRodStickState(ushort playerId, bool isOn)
        {
            GetPlayer(playerId).SetFishingRodStickState(isOn);
        }

        public void SetPlayerFishingRodStickDirection(ushort playerId, bool isDirectionRight)
        {
            GetPlayer(playerId).SetFishingRodStickDirection(isDirectionRight);
        }

        public UnityEngine.Vector2 GetPlayerFishingRodTipPivotPosition(ushort playerId)
        {
            return GetPlayer(playerId).GetFishingRodTipPivotPosition();
        }

        public void SetPlayerHeadbuttChargingState(ushort playerId, bool isCharging)
        {
            GetPlayer(playerId).SetHeadbuttChargingState(isCharging);
        }

        public void ShowPlayerHeadbuttHelmet(ushort playerId)
        {
            GetPlayer(playerId).ShowHeadbuttHelmet();
        }

        public void HidePlayerHeadbuttHelmet(ushort playerId)
        {
            GetPlayer(playerId).HideHeadbuttHelmet();
        }

        public void OnPlayerHeadbuttTalentDeactivated(ushort playerId)
        {
            GetPlayer(playerId).OnHeadbuttTalentDeactivated();
        }

        public void SetPlayerChickenState(ushort playerId, bool isChicken)
        {
            GetPlayer(playerId).SetChickenState(isChicken);
        }

        public void SetPlayerRockState(ushort playerId, bool isRock)
        {
            GetPlayer(playerId).SetRockState(isRock);
        }

        public void SetPlayerOnLavaEffectState(ushort playerId, bool isExposedToLava)
        {
            GetPlayer(playerId).SetOnLavaEffectState(isExposedToLava);
        }

        public void PlayLayEggAnimation(ushort playerId)
        {
            GetPlayer(playerId).PlayLayEggAnimation(_stageCancellationTokenProvider.CancellationTokenSource);
        }

        public void PlayerYearsOfPainForPlayer(ushort playerId, Vector2 direction)
        {
            GetPlayer(playerId).PlayerYearsOfPain(direction);
        }

        public void PlaySonicSnapEffectForPlayer(ushort playerId)
        {
            GetPlayer(playerId).PlaySonicSnapEffect();
        }

        public void ShowPowerUpEffect(ushort playerId)
        {
            GetPlayer(playerId).ShowActivatePowerUpEffect(_stageCancellationTokenProvider.CancellationTokenSource.Token).Forget();
        }

        public void StartPowerUpGrantingPhase(ushort playerId)
        {
            GetPlayer(playerId).StartPowerUpGrantingPhase(_stageCancellationTokenProvider.CancellationTokenSource.Token).Forget();
        }

        public void EndPowerUpGrantingPhase(ushort playerId, PowerUpType grantedPowerUp)
        {
            GetPlayer(playerId).EndPowerUpGrantingPhase(grantedPowerUp, _stageCancellationTokenProvider.CancellationTokenSource.Token).Forget();
        }

        public void SetIsDeadAuraEnabled(ushort playerId, bool isEnabled)
        {
            GetPlayer(playerId).SetIsDeadEffectEnabled(isEnabled);
        }

        public void SetPlayerIsLockOnTargetSightShown(ushort playerId, bool isShown)
        {
            GetPlayer(playerId).SetIsLockOnTargetSightShown(isShown);
        }

        public void SetIsPlayerKinged(ushort playerId, bool isKinged)
        {
            GetPlayer(playerId).SetIsKinged(isKinged);
        }

        public void RefreshLeaderFlags()
        {
            foreach (var playerController in _playerControllers)
            {
                var teamId = _matchDataService.GetPlayerTeamId(playerController.PlayerId);
                playerController.SetIsLeader(_matchDataService.IsTeamLeadingInGems(teamId));
            }
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

        public Transform GetPlayerHeartTransform(ushort playerId)
        {
            return GetPlayer(playerId).GetHeartTransform();
        }
        
        public Transform GetPlayerHeadTransform(ushort playerId)
        {
            return GetPlayer(playerId).GetHeadTransform();
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

        public void UpdatePlayerTalents(ushort playerId, Core.Scripts.Utils.CustomCollections.FixedOrderedList<Core.Game.Domains.GamePlay.Shared.S2CModels.TalentStateS2C> talents, int currentServerTick)
        {
            var selectedTalentIndex = _matchDataService.GetPlayer(playerId).Spaceship.TalentsState.SelectedTalentIndex;
            GetPlayer(playerId).UpdateTalents(talents, selectedTalentIndex, currentServerTick);
        }

        public void SetIsTailWaving(ushort playerId, bool isWaving)
        {
            GetPlayer(playerId).SetIsTailWaving(isWaving);
        }
    }
}