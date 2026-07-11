using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.SecondCastAimArrowEffect.Scripts
{
    public interface ISecondCastEffectController
    {
        void InitEntryPoint();

        /// <summary>
        /// Shows/updates the throw-aim arrow for a fishing rod projectile at the given position and direction.
        /// Keyed by the projectile (tip) id, so multiple rods catching the same enemy each get their own arrow.
        /// Call <see cref="RemoveArrow"/> to hide it (e.g. when the tip is no longer in the CaughtEnemy phase).
        /// </summary>
        void SetArrow(ushort tipId, Vector2 position, Vector2 direction);

        void RemoveArrow(ushort tipId);
        void DestroyAll();
    }
}
