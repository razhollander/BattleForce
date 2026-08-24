using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Models;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.Environment.GateTraps.Scripts.Mvc
{
    public interface IMatchEnvironmentGateTrapsControllers
    {
        void InitEntryPoint();
        void CreateGateTrap(MatchEnvironmentGateTrapModel gateTrapModel);
        void UpdateGateTrapViews();
        void DestroyAll();
    }
}
