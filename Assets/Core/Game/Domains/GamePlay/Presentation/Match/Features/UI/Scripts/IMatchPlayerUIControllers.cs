using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Scripts.Utils.CustomCollections;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.UI.Scripts
{
    public interface IMatchPlayerUIControllers
    {
        void AddPlayer(ushort playerId, int currentServerTick);
        void SetPlayerHealth(ushort playerId, ushort currentHealth, ushort maxHealth);
        void HidePlayerHealthBar(ushort playerId);
        void UpdatePlayerMolesKilledScore(ushort playerId, int molesKilledScore);
        void UpdatePlayerGatePassScore(ushort playerId, int gatePassScore);
        void SwitchToPlayerDeadState(ushort playerId);
        void DestroyAll();
        void UpdatePlayerTalents(ushort playerId, FixedOrderedList<TalentStateS2C> talents, int currentServerTick);
        void SetPlayerSelectedTalent(ushort playerId, int index);
        void UpdatePlayersTalentCooldowns(int currentServerTick);
    }
}