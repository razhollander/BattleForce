namespace Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager
{
    public interface IPowerUpsSpawnerService
    {
        void StepTimer(float deltaTime);
        void RestartSpawnTimer();
        bool IsSpawnTimerEnded();
    }
}