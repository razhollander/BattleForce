namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.UI.Scripts
{
    public interface IMatchPlayerUIControllers
    {
        void AddPlayer(ushort playerId);
        void SetPlayerHealth(ushort playerId, ushort currentHealth, ushort maxHealth);
        void HidePlayerHealthBar(ushort playerId);
        void SetPlayerOpacity(ushort playerId, float alpha);
        void DestroyAll();
    }
}