
namespace Core.Game.Domains.GamePlay.Presentation.Features.Player.Scripts.Mvc
{
    public interface IPlayerControllers
    {
        void InitEntryPoint();
        void CreatePlayer(ushort playerId);
        void UpdatePlayersTransform();
        void UpdatePlayersBulletCooldowns();
        void ShootBulletEffectForPlayer(ushort playerId);
        void SetPlayerHealth(ushort playerId, ushort currentHealth, ushort maxHealth);
    }
}