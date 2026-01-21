
using System.Numerics;

namespace Core.Game.Domains.GamePlay.Presentation.Features.Player.Scripts.Mvc
{
    public interface IPlayerControllers
    {
        void InitEntryPoint();
        void AddPlayer(ushort playerId);
        void UpdatePlayersTransform();
        void UpdatePlayersBulletCooldowns();
        void ShootBulletEffectForPlayer(ushort playerId);
        void SetPlayerHealth(ushort playerId, ushort currentHealth, ushort maxHealth);
        void SetPlayerTransform(ushort playerId, Vector2 position, Vector2 direction);
        UnityEngine.Vector2 GetPlayerPosition(ushort playerId);
    }
}