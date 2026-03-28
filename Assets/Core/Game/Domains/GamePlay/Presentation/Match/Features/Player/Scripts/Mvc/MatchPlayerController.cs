using Core.Game.Domains.GamePlay.Presentation.Features.Player.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Features.Player.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.DataService;
using Core.Game.Domains.GamePlay.Presentation.Scripts.ScriptableObjects;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Scripts.Extensions;
using Core.Scripts.Network;
using Core.Game.Domains.GamePlay.Shared.Scripts.Utils;
using CoreDomain.Scripts.Services.Logger.Base;
using UnityEngine;
using Vector2 = System.Numerics.Vector2;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.Player.Scripts.Mvc
{
    public class MatchPlayerController
    {
        private readonly IMatchDataService _matchDataService;
        private readonly PresentationGamePlayConfig _gamePlayConfig;
        private readonly NetworkConfig _networkConfig;
        private readonly Transform _parent;
        public readonly ushort PlayerId;
        private PlayerView _playerView;
        private readonly PlayerViewPool _playerPool;

        public MatchPlayerController(PlayerViewPool playerPool, ushort playerId, IMatchDataService matchDataService, PresentationGamePlayConfig gamePlayConfig, NetworkConfig networkConfig, Transform parent)
        {
            _playerPool = playerPool;
            _matchDataService = matchDataService;
            _gamePlayConfig = gamePlayConfig;
            _networkConfig = networkConfig;
            _parent = parent;
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
            var playerTransform = playerModel.Spaceship.Transform;
            _playerView.SetColor(_gamePlayConfig.ColorPerTeamId[playerModel.TeamId]);
            _playerView.SetPositionAndRotation(playerTransform.Position.ToUnityVector2(),
                playerTransform.Direction.ToUnityVector2().ToQuaternion());
            SetHealth(playerModel.Spaceship.Health.CurrentHealth, playerModel.Spaceship.Health.MaxHealth);
            var doesPlayerHaveAnyTalent = playerModel.Spaceship.TalentsState.Talents.Count > 0;
            if (doesPlayerHaveAnyTalent)
            {
                SetSelectedTalent(playerModel.Spaceship.TalentsState.SelectedTalentIndex);
            }
        }

        public void SetSelectedTalent(int talentIndex)
        {
            var playerModel = _matchDataService.GetPlayer(PlayerId);
            var talentState = playerModel.Spaceship.TalentsState.Talents[talentIndex];
            var talentSprite = _gamePlayConfig.TalentCards.TalentSprites[talentState.TalentType];
            _playerView.SetTalentSprite(talentSprite);
        }

        public void UpdateTransform()
        {
            var playerModel = _matchDataService.GetPlayer(PlayerId);
            var playerTransformState = playerModel.Spaceship.Transform;
            var playerPosition = playerTransformState.Position.ToUnityVector2();
            var playerRotation = playerTransformState.Direction.ToUnityVector2().ToQuaternion();
            var decay = _gamePlayConfig.ExponentialDecay;
            _playerView.InterpolateTransform(playerPosition, playerRotation, decay);
            _playerView.InterpolateAimRotation(playerModel.Spaceship.TalentsState.AimDirection, decay);
        }

        public void UpdateBulletCooldown()
        {
            var playerModel = _matchDataService.GetPlayer(PlayerId);
            var playerShootState = playerModel.Spaceship.Shoot;
            var maxShootCooldown = playerShootState.MaxCooldown;
            var cooldownSecondsLeft = playerShootState.CooldownSecondsLeft;
            var exponentialDecay = _gamePlayConfig.ExponentialDecay;
            _playerView.InterpolateBulletLoading(cooldownSecondsLeft, maxShootCooldown, exponentialDecay);
            if (Mathf.Approximately(cooldownSecondsLeft, maxShootCooldown))
            {
                RestoreBulletEffect();
            }
        }

        public void UpdateTalentCooldown(int currentServerTick)
        {
            var playerModel = _matchDataService.GetPlayer(PlayerId);
            var talentsState = playerModel.Spaceship.TalentsState;
            if (talentsState.Talents.Count == 0) return;
            var talentState = talentsState.GetCurrentSelectedTalent();

            float maxCooldown = 0;
            float cooldownLeft = 0;

            switch (talentState.CooldownType)
            {
                case TalentCooldownType.Normal:
                    maxCooldown = talentState.NormalCooldown.MaxCooldown;
                    cooldownLeft = talentState.NormalCooldown.IsOnCooldown() ? TickUtils.GetSecondsLeftUntilTick(currentServerTick, talentState.NormalCooldown.CooldownEndTick, _networkConfig.DeltaTime) : 0;
                    break;
                case TalentCooldownType.Stocks:
                    maxCooldown = talentState.StocksCooldown.MaxSingleStockCooldown;
                    cooldownLeft = talentState.StocksCooldown.IsAtMaxStocks() ? 0 : TickUtils.GetSecondsLeftUntilTick(currentServerTick, talentState.StocksCooldown.RecieveNextStockOnTick, _networkConfig.DeltaTime);
                    break;
                default:
                    LogService.LogError("Not implemented cooldown type: " + talentState.CooldownType);
                    break;
            }

            // Fallback for division by zero if maxCooldown is somehow 0
            if (maxCooldown <= 0.001f) maxCooldown = 1f;

            var exponentialDecay = _gamePlayConfig.ExponentialDecay;
            _playerView.InterpolateTalentLoading(cooldownLeft, maxCooldown, exponentialDecay);
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
    }
}