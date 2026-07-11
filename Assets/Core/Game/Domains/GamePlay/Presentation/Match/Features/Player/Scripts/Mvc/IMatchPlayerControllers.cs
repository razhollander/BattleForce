using Core.Game.Domains.GamePlay.Shared.S2CModels;
using UnityEngine;
using Vector2 = System.Numerics.Vector2;
using CoreDomain.Scripts.Services.UpdateService;


namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.Player.Scripts.Mvc
{
    public interface IMatchPlayerControllers
    {
        void InitEntryPoint();
        void AddPlayer(ushort playerId);
        void UpdatePlayersTickDeltas();
        void UpdatePlayersBulletCooldowns();
        void UpdatePlayersTalentCooldowns(int currentServerTick);
        void ShootBulletEffectForPlayer(ushort playerId);
        void SetPlayerHealth(ushort playerId, ushort currentHealth, ushort maxHealth);
        void SetPlayerTransform(ushort playerId, Vector2 position, Vector2 direction);
        UnityEngine.Vector2 GetPlayerPosition(ushort playerId);
        Transform GetPlayerSpaceshipTransform(ushort playerId);
        Transform GetPlayerTransform(ushort playerId);
        Transform GetPlayerHeartTransform(ushort playerId);
        Transform GetPlayerHeadTransform(ushort playerId);
        void HidePlayerHealthBar(ushort playerId);
        void DestroyAll();
        void SetPlayerTalentSelected(ushort playerId, int talentIndex);
        void UpdatePlayerTalents(ushort playerId, Core.Scripts.Utils.CustomCollections.FixedOrderedList<Core.Game.Domains.GamePlay.Shared.S2CModels.TalentStateS2C> talents, int currentServerTick);
        void SetIsTailWaving(ushort playerId, bool isWaving);
        void SetPlayerSentryGunState(ushort playerId, bool isOn);
        void SetPlayersSpinnedState(ushort playerId, bool isOn);
        void SetPlayerUmbrellaState(ushort playerId, bool isOn);
        void SetPlayerCurrentPowerUp(ushort playerId, PowerUpType powerUpType);
        void SetPlayerWaterGunState(ushort playerId, bool isOn);
        void SetPlayerFishingRodStickState(ushort playerId, bool isOn);
        void SetPlayerFishingRodStickDirection(ushort playerId, bool isDirectionRight);
        UnityEngine.Vector2 GetPlayerFishingRodTipPivotPosition(ushort playerId);
        void SetPlayerHeadbuttChargingState(ushort playerId, bool isCharging);
        void ShowPlayerHeadbuttHelmet(ushort playerId);
        void StartPlayerHeadbuttDashHelmetHideTimer(ushort playerId);
        void HidePlayerHeadbuttHelmet(ushort playerId);
        void OnPlayerHeadbuttTalentDeactivated(ushort playerId);
        void SetPlayerChickenState(ushort playerId, bool isOn);
        void SetPlayerRockState(ushort playerId, bool isRock);
        void SetPlayerOnLavaEffectState(ushort playerId, bool isExposedToLava);
        void PlayLayEggAnimation(ushort playerId);
        void PlayerYearsOfPainForPlayer(ushort playerId, Vector2 direction);
        void PlaySonicSnapEffectForPlayer(ushort playerId);
        void ShowPowerUpEffect(ushort playerId);
        void StartPowerUpGrantingPhase(ushort playerId);
        void EndPowerUpGrantingPhase(ushort playerId, PowerUpType grantedPowerUp);
        void SetIsDeadAuraEnabled(ushort playerId, bool isEnabled);
        void SetPlayerIsLockOnTargetSightShown(ushort playerId, bool isShown);
        void SetIsPlayerKinged(ushort playerId, bool isKinged);
        void RefreshLeaderFlags();
    }
}