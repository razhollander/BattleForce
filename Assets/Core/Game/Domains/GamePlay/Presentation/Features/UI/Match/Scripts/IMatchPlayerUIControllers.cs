namespace Core.Game.Domains.GamePlay.Presentation.Features.UI.Match.Scripts
{
    public interface IMatchPlayerUIControllers
    {
        void AddPlayer(ushort playerId);
        void SetPlayerHealth(ushort playerId, ushort currentHealth, ushort maxHealth);
    }
}