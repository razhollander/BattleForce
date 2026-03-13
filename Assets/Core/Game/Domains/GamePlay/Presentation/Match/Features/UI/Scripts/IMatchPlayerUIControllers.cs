using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Scripts.Utils.CustomCollections;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.UI.Scripts
{
    public interface IMatchPlayerUIControllers
    {
        void AddPlayer(ushort playerId);
        void SetPlayerHealth(ushort playerId, ushort currentHealth, ushort maxHealth);
        void HidePlayerHealthBar(ushort playerId);
        void SwitchToPlayerDeadState(ushort playerId);
        void DestroyAll();
        void UpdatePlayerTalents(ushort playerId, FixedOrderedList<TalentStateS2C> talents);
        void SetPlayerSelectedTalent(ushort playerId, int index);
        void UpdatePlayersTalentsCooldowns();
    }
}