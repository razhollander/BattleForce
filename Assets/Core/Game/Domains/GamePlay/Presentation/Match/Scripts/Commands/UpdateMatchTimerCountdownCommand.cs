using Core.Game.Domains.GamePlay.Presentation.Match.Features.MatchTimerCountdown.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.DataService;
using Core.Game.Domains.GamePlay.Shared.Scripts.Enums;
using Core.Scripts.Network;
using CoreDomain.Scripts.Services.CommandFactory;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands
{
    public class UpdateMatchTimerCountdownCommand : BaseCommand, ICommandVoid
    {
        private IMatchDataService _matchDataService;
        private IMatchTimerCountdownController _matchTimerCountdownController;
        private NetworkConfig _networkConfig;

        private int _tick;

        public UpdateMatchTimerCountdownCommand SetTick(int tick)
        {
            _tick = tick;
            return this;
        }

        public override void ResolveDependencies()
        {
            _matchDataService = _diContainer.Resolve<IMatchDataService>();
            _matchTimerCountdownController = _diContainer.Resolve<IMatchTimerCountdownController>();
            _networkConfig = _diContainer.Resolve<NetworkConfig>();
        }

        public void Execute()
        {
            // The preparation phase has its own centered countdown, so this one only takes over once the stage is live.
            // Shown for every bonus stage (Whac-A-Mole + GatePass).
            var shouldShowCountdown = _matchDataService.StageType.IsBonusStage() && !_matchDataService.IsInPreparationPhase;

            if (!shouldShowCountdown)
            {
                _matchTimerCountdownController.Hide();
                return;
            }

            _matchTimerCountdownController.Show();

            var ticksLeft = Mathf.Max(0, _matchDataService.WhacAMoleEndTick - _tick);
            var secondsLeft = Mathf.CeilToInt(ticksLeft * _networkConfig.DeltaTime);
            _matchTimerCountdownController.SetSecondsLeft(secondsLeft);
        }
    }
}
