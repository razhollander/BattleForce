using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.Nuke.Scripts
{
    public interface INukeShockwaveEffectController
    {
        void InitEntryPoint();
        void PlayEffect(Vector2 position);
    }
}
