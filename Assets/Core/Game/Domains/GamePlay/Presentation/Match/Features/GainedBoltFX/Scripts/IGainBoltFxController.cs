using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.FX.Scripts
{
    public interface IGainBoltFxController
    {
        void InitEntryPoint();
        void ShowFx(int amount, Vector2 position);
    }
}
