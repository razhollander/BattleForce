using Core.Game.Domains.GamePlay.Shared.S2CModels;
using UnityEngine;
using Vector2 = System.Numerics.Vector2;
using CoreDomain.Scripts.Services.UpdateService;


namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.Player.Scripts.Mvc
{
    public interface IMatchPlayerControllers : IGUIUpdatable
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
        void HidePlayerHealthBar(ushort playerId);
        void DestroyAll();
        void ManagedUpdate();
        void SetPlayerTalentSelected(ushort playerId, int talentIndex);
        void UpdatePlayerTalents(ushort playerId, Core.Scripts.Utils.CustomCollections.FixedOrderedList<Core.Game.Domains.GamePlay.Shared.S2CModels.TalentStateS2C> talents, int currentServerTick);
        void SetIsTailWaving(ushort playerId, bool isWaving);
        void SetPlayerSentryGunState(ushort playerId, bool isOn);
        void SetPlayersSpinnedState(ushort playerId, bool isOn);
        void SetPlayerUmbrellaState(ushort playerId, bool isOn);
        void SetPlayerChickenState(ushort playerId, bool isOn);
        void PlayLayEggAnimation(ushort playerId);
        void PlayerYearsOfPainForPlayer(ushort playerId, Vector2 direction);
        void SetIsDeadAuraEnabled(ushort playerId, bool isEnabled);
    }
}