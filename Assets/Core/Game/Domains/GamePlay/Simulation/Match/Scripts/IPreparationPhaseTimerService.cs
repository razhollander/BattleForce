namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts
{
    public interface IPreparationPhaseTimerService
    {
        float PreparationPhaseTimer { get; set; }
        float PreparationPhaseSecondsElapsed { get; }
        void StepPreperationPhaseTimer(float deltaTime);
        bool IsTimerCompleted();
        void RestartTimer();
    }
}