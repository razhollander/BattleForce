namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MolesSpawner
{
    public interface IMolesSpawnerService
    {
        void StepTimer(float deltaTime);
        void RestartSpawnTimer();
        bool IsSpawnTimerEnded();
    }
}
