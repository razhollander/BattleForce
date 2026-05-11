using System.Threading;
using Core.Game.Domains.GamePlay.Presentation.Features.Player.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.DataService;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Models;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.StageCancellationToken;
using Core.Game.Domains.GamePlay.Presentation.Scripts.InputBeingUsed;
using Core.Game.Domains.GamePlay.Presentation.Scripts.ScriptableObjects;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Shared.Scripts.Enums;
using Core.Scripts.Extensions;
using Core.Scripts.Network;
using Core.Game.Domains.GamePlay.Shared.Scripts.Utils;
using CoreDomain.Scripts.Services.Logger.Base;
using UnityEngine;
using UnityEngine.InputSystem.HID;
using Vector2 = System.Numerics.Vector2;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.Player.Scripts.Mvc
{
    public class MatchPlayerController
    {
        private readonly IMatchDataService _matchDataService;
        private readonly PresentationGamePlayConfig _gamePlayConfig;
        private readonly NetworkConfig _networkConfig;
        private readonly Transform _parent;
        private readonly IStageCancellationTokenProvider _stageCancellationTokenProvider;
        private readonly IInputBeingUsedService _inputBeingUsedService;
        public readonly ushort PlayerId;
        private PlayerView _playerView;
        private readonly PlayerViewPool _playerPool;

        public MatchPlayerController(PlayerViewPool playerPool, ushort playerId, IMatchDataService matchDataService, PresentationGamePlayConfig gamePlayConfig,
            NetworkConfig networkConfig, Transform parent, IStageCancellationTokenProvider stageCancellationTokenProvider, IInputBeingUsedService inputBeingUsedService) 
        {
            _playerPool = playerPool;
            _matchDataService = matchDataService;
            _gamePlayConfig = gamePlayConfig;
            _networkConfig = networkConfig;
            _parent = parent;
            _stageCancellationTokenProvider = stageCancellationTokenProvider;
            _inputBeingUsedService = inputBeingUsedService;
            PlayerId = playerId;
        }

        public void CreatePlayerView()
        {
            var playerModel = _matchDataService.GetPlayer(PlayerId);
            var playerName = playerModel.PlayerName;
            _playerView = _playerPool.Spawn();
            _playerView.transform.SetParent(_parent);
            _playerView.name = "Player_" + PlayerId + "_" + playerName;
            _playerView.SetPlayerName(playerName);
            _playerView.SetIsTailWaving(true);
            var playerTransform = playerModel.Spaceship.Transform;
            _playerView.SetColor(_gamePlayConfig.ColorPerTeamId[playerModel.TeamId]);
            _playerView.SetPositionAndRotation(playerTransform.Position.ToUnityVector2(),
                playerTransform.Direction.ToUnityVector2().ToQuaternion());
            SetHealth(playerModel.Spaceship.Health.CurrentHealth, playerModel.Spaceship.Health.MaxHealth);
            var isDead = playerModel.Spaceship.Health.CurrentHealth == 0;
            SetIsDeadAuraEnabled(isDead);
            SetupPlayerAccordingToHisSelectedTalent(playerModel);
            SetPlayersSpinnedState(playerModel.Spaceship.IsSpinned);
        }

        private void SetupPlayerAccordingToHisSelectedTalent(MatchPlayerModel playerModel)
        {
            if (!playerModel.Spaceship.TalentsState.TryGetCurrentSelectedTalent(out var currentSelectedTalentState))
            {
                return;
            }

            SetSelectedTalent(playerModel.Spaceship.TalentsState.SelectedTalentIndex);

            if (currentSelectedTalentState.TalentType == TalentType.SentryGun)
            {
                SetSentryGunState(currentSelectedTalentState.IsCurrentlyActive, _stageCancellationTokenProvider.CancellationTokenSource);
            }
            else if (currentSelectedTalentState.TalentType == TalentType.Umbrella)
            {
                SetUmbrellaState(currentSelectedTalentState.IsCurrentlyActive);
            }
            else if (currentSelectedTalentState.TalentType == TalentType.Chicken)
            {
                SetChickenState(true);
            }
        }

        public void SetSentryGunState(bool isSentryGun, CancellationTokenSource cancellationTokenSource)
        {
            _playerView.SetSentryGunState(isSentryGun, cancellationTokenSource);
        }

        public void PlayLayEggAnimation(CancellationTokenSource cancellationTokenSource)
        {
            _playerView.PlayLayEggAnimation(cancellationTokenSource);
        }

        public void SetChickenState(bool isChickenActive)
        {
            _playerView.SetChickenState(isChickenActive);
        }

        public void SetUmbrellaState(bool isUmbrellaActive)
        {
            _playerView.SetUmbrellaState(isUmbrellaActive);
        }
        
        public void SetSelectedTalent(int talentIndex)
        {
            var playerModel = _matchDataService.GetPlayer(PlayerId);
            var talentState = playerModel.Spaceship.TalentsState.Talents[talentIndex];
            var talentType = talentState.TalentType;
            var talentSprite = _gamePlayConfig.TalentCards.TalentSprites[talentType];
            _playerView.SetTalentSprite(talentSprite);
            var isInChickenState = talentType == TalentType.Chicken;
            SetChickenState(isInChickenState);
        }

        public void UpdateTickDeltas()
        {
            var playerModel = _matchDataService.GetPlayer(PlayerId);
            var playerTransformState = playerModel.Spaceship.Transform;
            var playerPosition = playerTransformState.Position.ToUnityVector2();
            var playerRotation = playerTransformState.Direction.ToUnityVector2().ToQuaternion();
            var decay = _gamePlayConfig.ExponentialDecay;
            var aimDirection = playerModel.Spaceship.TalentsState.AimDirection;
            _playerView.InterpolateTransform(playerPosition, playerRotation, decay);
            _playerView.UpdateTailBend();
            UpdateAim(playerModel.Spaceship.AssistArrowType, aimDirection, decay);

            if (playerModel.Spaceship.TalentsState.TryGetCurrentSelectedTalent(out var selectedTalent) &&
                selectedTalent.TalentType == TalentType.Umbrella)
            {
                _playerView.InterpolateUmbrellaRotation(aimDirection, decay);
            }
        }

        private void UpdateAim(PlayerAssistArrowType arrowType, Vector2 aimDirection, float decay)
        {
            _playerView.InterpolateAimRotation(aimDirection, decay);

            var shouldShowMoveAssistArrow = arrowType == PlayerAssistArrowType.Hidden && _inputBeingUsedService.InputTypeBeingUsed == SupportedInputType.GamePad;

            if (shouldShowMoveAssistArrow)
            {
                _playerView.ShowMoveAssistArrow();
                return;
            }

            var shouldShowAimArrow = arrowType == PlayerAssistArrowType.AimArrow;
            if (shouldShowAimArrow)
            {
                _playerView.ShowAimAssistArrow();
                return;
            }

            _playerView.HideAssistArrow();
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

        public void UpdateTalentCooldown(int currentServerTick)
        {
            var playerModel = _matchDataService.GetPlayer(PlayerId);
            var talentsState = playerModel.Spaceship.TalentsState;
            if (!talentsState.TryGetCurrentSelectedTalent(out var currentSelectedTalentState))
            {
                return;
            }
            

            float maxCooldown = 0;
            float cooldownLeft = 0;

            switch (currentSelectedTalentState.CooldownType)
            {
                case TalentCooldownType.Normal:
                    maxCooldown = currentSelectedTalentState.NormalCooldown.MaxCooldown;
                    cooldownLeft = currentSelectedTalentState.NormalCooldown.IsOnCooldown() ? TickUtils.GetSecondsLeftUntilTick(currentServerTick, currentSelectedTalentState.NormalCooldown.CooldownEndTick, _networkConfig.DeltaTime) : 0;
                    break;
                case TalentCooldownType.Stocks:
                    maxCooldown = currentSelectedTalentState.StocksCooldown.MaxSingleStockCooldown;
                    cooldownLeft = currentSelectedTalentState.StocksCooldown.CurrentStocksAmount > 0 ? 0 : TickUtils.GetSecondsLeftUntilTick(currentServerTick, currentSelectedTalentState.StocksCooldown.RecieveNextStockOnTick, _networkConfig.DeltaTime);
                    break;
                case TalentCooldownType.AlwaysActive:
                    maxCooldown = 0;
                    cooldownLeft = 0;
                    break;
                default:
                    LogService.LogError("Not implemented cooldown type: " + currentSelectedTalentState.CooldownType);
                    break;
            }
            
            _playerView.SetTalentLoading(cooldownLeft, maxCooldown);
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

        public Transform GetSpaceShipTransform()
        {
            return _playerView.GetSpaceShipTransform();
        }

        public void SetIsHealthBarShown(bool isShown)
        {
            _playerView.SetIsHealthBarShown(isShown);
        }

        public void Destroy()
        {
            _playerView.Despawn();
        }

        public Transform GetTransform()
        {
            return _playerView.GetTransform();
        }

        public void SetIsTailWaving(bool isMoving)
        {
            _playerView.SetIsTailWaving(isMoving);
        }

        public void SetPlayersSpinnedState(bool isOn)
        {
            _playerView.SetIsSpinned(isOn, _stageCancellationTokenProvider.CancellationTokenSource);
        }

        public void PlayerYearsOfPain(Vector2 direction)
        {
            _playerView.PlayYearsOfPainAnimation(direction.ToUnityVector2(), _stageCancellationTokenProvider.CancellationTokenSource);
        }

        public void SetIsDeadAuraEnabled(bool isEnabled)
        {
            _playerView.SetIsDeadAuraEnabled(isEnabled);
        }
    }
}