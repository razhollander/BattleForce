using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.MoveDestinationPointIndicator.Scripts
{
    public interface IMoveDestinationPointIndicatorController
    {
        void InitEntryPoint();
        void ShowIndicator(Vector2 destinationPoint);
    }
}
