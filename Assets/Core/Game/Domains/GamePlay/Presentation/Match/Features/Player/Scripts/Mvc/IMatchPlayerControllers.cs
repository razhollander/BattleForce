using Core.Game.Domains.GamePlay.Shared.S2CModels;
using UnityEngine;
using Vector2 = System.Numerics.Vector2;

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
        void HidePlayerHealthBar(ushort playerId);
        void DestroyAll();
        void SetPlayerTalentSelected(ushort playerId, int talentIndex);
        void SetIsTailWaving(ushort playerId, bool isWaving);
        void SetPlayerSentryGunState(ushort playerId, bool isOn);
        void SetPlayersSpinnedState(ushort playerId, bool isOn);
        void UpdateIsPlayerArrowShownAccordingToTalentState(ushort playerId, TalentStateS2C talentStateS2C);
        void SetPlayerUmbrellaState(ushort playerId, bool isOn);
        void SetPlayerChickenState(ushort playerId, bool isOn);
        void SetPlayerRockState(ushort playerId, bool isOn);
        void PlayLayEggAnimation(ushort playerId);
    }
}