using Core.Game.Domains.GamePlay.Presentation.Match.Features.UI.Scripts;
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
        private MatchPlayerView _playerView;
        private readonly MatchPlayerViewPool _playerPool;

        public MatchPlayerController(MatchPlayerViewPool playerPool, ushort playerId, IMatchDataService matchDataService, PresentationGamePlayConfig gamePlayConfig,
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
            _playerView.Base.SetPlayerName(playerName);
            _playerView.Base.SetIsTailWaving(true);
            var playerTransform = playerModel.Spaceship.Transform;
            _playerView.Base.SetColor(_gamePlayConfig.ColorPerTeamId[playerModel.TeamId]);
            _playerView.Base.SetPositionAndRotation(playerTransform.Position.ToUnityVector2(),
                playerTransform.Direction.ToUnityVector2().ToQuaternion());
            SetHealth(playerModel.Spaceship.Health.CurrentHealth, playerModel.Spaceship.Health.MaxHealth);
            var isDead = playerModel.Spaceship.Health.CurrentHealth == 0;
            SetIsDeadAuraEnabled(isDead);
            UpdateTalents(playerModel.Spaceship.TalentsState.Talents, playerModel.Spaceship.TalentsState.SelectedTalentIndex, 0);
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
            _playerView.SetSelectedTalent(talentIndex);
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
            _playerView.Base.InterpolateTransform(playerPosition, playerRotation, decay);
            _playerView.Base.UpdateTailBend();
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
            _playerView.Base.SetBulletLoading(cooldownSecondsLeft, maxShootCooldown);
            if (Mathf.Approximately(cooldownSecondsLeft, maxShootCooldown))
            {
                RestoreBulletEffect();
            }
        }

        public void UpdateTalentCooldown(int currentServerTick)
        {
            var playerModel = _matchDataService.GetPlayer(PlayerId);
            var talentsState = playerModel.Spaceship.TalentsState;

            for (int i = 0; i < talentsState.Talents.Count; i++)
            {
                var talentState = talentsState.Talents[i];

                switch (talentState.CooldownType)
                {
                    case TalentCooldownType.Normal:
                        var maxCooldown = talentState.NormalCooldown.MaxCooldown;
                        var isOnCooldown = talentState.NormalCooldown.IsOnCooldown();
                        var cooldownLeft = isOnCooldown ? TickUtils.GetSecondsLeftUntilTick(currentServerTick, talentState.NormalCooldown.CooldownEndTick, _networkConfig.DeltaTime) : 0;
                        _playerView.UpdateTalentCooldown(i, maxCooldown, cooldownLeft, isOnCooldown);
                        break;
                    case TalentCooldownType.Stocks:
                        var maxCooldownStocks = talentState.StocksCooldown.MaxSingleStockCooldown;
                        var isOnCooldownStocks = talentState.StocksCooldown.IsOnCooldown();
                        var cooldownLeftStocks = talentState.StocksCooldown.IsAtMaxStocks() ? 0 : TickUtils.GetSecondsLeftUntilTick(currentServerTick, talentState.StocksCooldown.RecieveNextStockOnTick, _networkConfig.DeltaTime);
                        _playerView.UpdateTalentCooldown(i, maxCooldownStocks, cooldownLeftStocks, isOnCooldownStocks);
                        _playerView.UpdateTalentStocks(i, talentState.StocksCooldown.CurrentStocksAmount);
                        break;
                    case TalentCooldownType.AlwaysActive:
                        break;
                    default: LogService.LogError("Not implemented cooldown type: " + talentState.CooldownType);
                        break;
                }
            }

            // Also keep updating the loading ring based on the current selected talent if needed, or remove SetTalentLoading
            // Wait, we still need SetTalentLoading for the loading ring maybe?
            // Yes, let's keep SetTalentLoading for the currently selected talent.
            if (!talentsState.TryGetCurrentSelectedTalent(out var currentSelectedTalentState))
            {
                return;
            }

            float maxCooldownRing = 0;
            float cooldownLeftRing = 0;

            switch (currentSelectedTalentState.CooldownType)
            {
                case TalentCooldownType.Normal:
                    maxCooldownRing = currentSelectedTalentState.NormalCooldown.MaxCooldown;
                    cooldownLeftRing = currentSelectedTalentState.NormalCooldown.IsOnCooldown() ? TickUtils.GetSecondsLeftUntilTick(currentServerTick, currentSelectedTalentState.NormalCooldown.CooldownEndTick, _networkConfig.DeltaTime) : 0;
                    break;
                case TalentCooldownType.Stocks:
                    maxCooldownRing = currentSelectedTalentState.StocksCooldown.MaxSingleStockCooldown;
                    cooldownLeftRing = currentSelectedTalentState.StocksCooldown.CurrentStocksAmount > 0 ? 0 : TickUtils.GetSecondsLeftUntilTick(currentServerTick, currentSelectedTalentState.StocksCooldown.RecieveNextStockOnTick, _networkConfig.DeltaTime);
                    break;
                case TalentCooldownType.AlwaysActive:
                    maxCooldownRing = 0;
                    cooldownLeftRing = 0;
                    break;
            }
            
            _playerView.Base.SetTalentLoading(cooldownLeftRing, maxCooldownRing);
        }

        public void RestoreBulletEffect()
        {
            _playerView.Base.ShowIsBulletAvailable(true);
        }
        
        public void DoShootEffect()
        {
            _playerView.Base.ShowIsBulletAvailable(false);
            _playerView.MakeAngryForShortDuration(_stageCancellationTokenProvider.CancellationTokenSource.Token);
        }

        public void SetHealth(ushort currentHealth, ushort maxHealth)
        {
            _playerView.UpdateHealthBar(currentHealth, maxHealth);
        }

        public void SetTransform(Vector2 position, Vector2 direction)
        {
            _playerView.Base.SetPositionAndRotation(position.ToUnityVector2(), direction.ToUnityVector2().ToQuaternion());
        }

        public UnityEngine.Vector2 GetPosition()
        {
            return _playerView.Base.GetPosition();
        }

        public Transform GetSpaceShipTransform()
        {
            return _playerView.Base.GetSpaceShipTransform();
        }

        public void SetIsHealthBarShown(bool isShown)
        {
            _playerView.SetIsHealthBarShown(isShown);
        }

        public void Destroy()
        {
            _playerView.Base.Despawn();
        }

        public Transform GetTransform()
        {
            return _playerView.Base.GetTransform();
        }

        public void SetIsTailWaving(bool isMoving)
        {
            _playerView.Base.SetIsTailWaving(isMoving);
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

        private TalentVisualData[] ConvertTalentsToVisualData(Core.Scripts.Utils.CustomCollections.FixedOrderedList<TalentStateS2C> talents, int currentServerTick)
        {
            var talentsVisualData = new TalentVisualData[talents.Count];

            for (int i = 0; i < talentsVisualData.Length; i++)
            {
                var talentVisualData = new TalentVisualData();
                var talentState = talents[i];
                talentVisualData.Icon = _gamePlayConfig.TalentCards.TalentSprites[talentState.TalentType];

                switch (talentState.CooldownType)
                {
                    case TalentCooldownType.Normal:
                        var isOnCooldown = talentState.IsOnCooldown();
                        talentVisualData.IsOnCooldown = isOnCooldown;
                        talentVisualData.IsStockable = false;
                        talentVisualData.CooldownLeft = isOnCooldown ? TickUtils.GetSecondsLeftUntilTick(currentServerTick, talentState.NormalCooldown.CooldownEndTick, _networkConfig.DeltaTime) : 0;
                        break;
                    case TalentCooldownType.Stocks:
                        var maxCooldown2 = talentState.StocksCooldown.MaxSingleStockCooldown;
                        var isOnCooldown2 = talentState.StocksCooldown.IsOnCooldown();
                        var cooldownLeft2 = talentState.StocksCooldown.IsAtMaxStocks() ? 0 : TickUtils.GetSecondsLeftUntilTick(currentServerTick, talentState.StocksCooldown.RecieveNextStockOnTick, _networkConfig.DeltaTime);
                        talentVisualData.IsStockable = true;
                        talentVisualData.StocksAmount = talentState.StocksCooldown.CurrentStocksAmount;
                        talentVisualData.CooldownLeft = cooldownLeft2;
                        talentVisualData.MaxCooldown = maxCooldown2;
                        talentVisualData.IsOnCooldown = isOnCooldown2;
                        break;
                    case TalentCooldownType.AlwaysActive:
                        talentVisualData.CooldownLeft = 1;
                        talentVisualData.MaxCooldown = 1;
                        talentVisualData.IsOnCooldown = true;
                        break;
                    default: LogService.LogError("Not implemented cooldown type: " + talentState.CooldownType);
                        break;
                }
                talentsVisualData[i] = talentVisualData;
            }

            return talentsVisualData;
        }

        public void UpdateTalents(Core.Scripts.Utils.CustomCollections.FixedOrderedList<TalentStateS2C> talents, int selectedTalentIndex, int currentServerTick)
        {
            _playerView.UpdateTalents(ConvertTalentsToVisualData(talents, currentServerTick));
            _playerView.SetSelectedTalent(selectedTalentIndex);
        }

    }
}