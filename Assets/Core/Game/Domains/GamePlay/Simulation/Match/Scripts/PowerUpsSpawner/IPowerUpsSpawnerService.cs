namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.PowerUpsSpawner
{
    public interface IPowerUpsSpawnerService
    {
        void StepTimer(float deltaTime);
        void RestartSpawnTimer();
        bool IsSpawnTimerEnded();
    }
}