using Core.Scripts.Network;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.PreparationPhaseCountdown.Scripts
{
    public class PreparationPhaseCountdownController : IPreparationPhaseCountdownController
    {
        private readonly PreparationPhaseCountdownView _view;
        private readonly NetworkConfig _networkConfig;

        public PreparationPhaseCountdownController(PreparationPhaseCountdownView view, NetworkConfig networkConfig)
        {
            _view = view;
            _networkConfig = networkConfig;
        }

        public void PlayCountdown(int elapsedTicks)
        {
            var elapsedTimeInSeconds = elapsedTicks * _networkConfig.DeltaTime;
            _view.Play(elapsedTimeInSeconds);
        }

        public void StopCountdown()
        {
            _view.Stop();
        }
    }
}
