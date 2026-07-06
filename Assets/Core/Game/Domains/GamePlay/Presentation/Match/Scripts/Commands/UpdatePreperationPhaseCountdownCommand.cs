using Core.Game.Domains.GamePlay.Presentation.Match.Features.PreparationPhaseCountdown.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.DataService;
using Core.Scripts.Network;
using CoreDomain.Scripts.Services.CommandFactory;
using CoreDomain.Scripts.Services.Logger.Base;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands
{
    public class UpdatePreperationPhaseCountdownCommand : BaseCommand, ICommandVoid
    {
        private IMatchDataService _matchDataService;
        private IPreparationPhaseCountdownController _preparationPhaseCountdownController;
        private int _tick;
        private NetworkConfig _networkConfig;

        public UpdatePreperationPhaseCountdownCommand SetTick(int tick)
        {
            _tick = tick;
            return this;
        }

        public override void ResolveDependencies()
        {
            _matchDataService = _diContainer.Resolve<IMatchDataService>();
            _preparationPhaseCountdownController = _diContainer.Resolve<IPreparationPhaseCountdownController>();
            _networkConfig = _diContainer.Resolve<NetworkConfig>();

        }

        public void Execute()
        {
            if (!_matchDataService.IsInPreparationPhase)
            {
                if (_preparationPhaseCountdownController.IsCountdownPlaying)
                {
                    _preparationPhaseCountdownController.StopCountdown();
                }
                return;
            }

            var ticksPassedSincePreparationPhaseStarted = _tick - _matchDataService.PreperationPhaseStartedOnTick;
            var secondsPassedSincePreparationPhaseStarted = ticksPassedSincePreparationPhaseStarted*_networkConfig.DeltaTime;
            _preparationPhaseCountdownController.SetCountdownTime(secondsPassedSincePreparationPhaseStarted);
        }
    }
}