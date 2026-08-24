using Core.Game.Domains.GamePlay.Presentation.Match.Features.Environment.GateTraps.Scripts.Mvc;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands
{
    /// <summary>
    /// The gate trap models are advanced at packet-receive time; this only pushes whatever they hold now into the views.
    /// </summary>
    public class UpdateGateTrapsViewCommand : BaseCommand, ICommandVoid
    {
        private IMatchEnvironmentGateTrapsControllers _gateTrapsControllers;

        public override void ResolveDependencies()
        {
            _gateTrapsControllers = _diContainer.Resolve<IMatchEnvironmentGateTrapsControllers>();
        }

        public void Execute()
        {
            _gateTrapsControllers.UpdateGateTrapViews();
        }
    }
}
