namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MolesSpawner
{
    public interface IMolesSpawnTimerService
    {
        void StepTimer(float deltaTime);
        void RestartSpawnTimer();
        bool IsSpawnTimerEnded();
    }
}
